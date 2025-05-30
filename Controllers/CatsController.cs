using CatVault.DTOs;
using CatVault.Helpers;
using CatVault.Services;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace CatVault.Controllers
{
    [ApiController]
    [Route("api/cats")]
    public class CatsController : Controller
    {
        private readonly IBackgroundJobClient _jobs;
        private readonly IConfiguration _config;
        private readonly ILogger<CatsController> _logger;
        private readonly CatDbContext _db;
        public CatsController(IBackgroundJobClient jobs, IConfiguration config, ILogger<CatsController> logger, CatDbContext db)
        {
            _jobs = jobs;
            _config = config;
            _logger = logger;
            _db = db;
        }
        [HttpPost("fetch")]
        public IActionResult EnqueueFetch()
        {
            try
            {
                var apiKey = _config["MySettings:ApiKey"];
                var apiUrl = _config["MySettings:ApiUrl"];
                var jobId = _jobs.Enqueue<ICatFetchService>(svc => svc.FetchAndStoreAsync(apiKey, apiUrl, 25));
                return Ok(new { JobId = jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue fetch images.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CatDto>> GetById(int id)
        {
            try
            {
                if (_db.Cats == null) { return NotFound(); }
                var cat = await _db.Cats.Include(c => c.CatTags).ThenInclude(ct => ct.TagEntity).FirstOrDefaultAsync(c => c.Id == id);
                if (cat == null) return NotFound($"Cat with id: {id} was not found.");
                return Ok(ApiHelper.MapToDto(cat));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get cat by ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<CatDto>>> GetAll([FromQuery] string tag, [FromQuery, Range(1, int.MaxValue)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            try
            {
                var query = _db.Cats.AsQueryable();
                if (!string.IsNullOrEmpty(tag))
                {
                    query = query.Where(c => c.CatTags.Any(ct => ct.TagEntity.Name == tag));
                }
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(c => c.CatTags).ThenInclude(ct => ct.TagEntity)
                    .ToListAsync();
                var dtos = items.Select(ApiHelper.MapToDto).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all cats.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetImage(int id)
        {
            try
            {
                var img = await _db.Cats
                .Where(c => c.Id == id)
                .Select(c => c.Image)
                .FirstOrDefaultAsync();
                if (img == null) return NotFound();
                return File(img, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cat image.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
