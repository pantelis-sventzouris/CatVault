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
        private readonly IDataService _dataService;
        public CatsController(IBackgroundJobClient jobs, IConfiguration config, ILogger<CatsController> logger, IDataService dataService)
        {
            _jobs = jobs;
            _config = config;
            _logger = logger;
            _dataService = dataService;
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
                var cat = await _dataService.GetCatByIdAsync(id);
                if (cat == null) { return NotFound($"Cat with id: {id} was not found."); }
                return Ok(cat);
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
                if (page < 1 || pageSize < 1 || pageSize > 100)
                {
                    return BadRequest("Page and pageSize must be positive integers, and pageSize cannot exceed 100.");
                }
                if (string.IsNullOrEmpty(tag)) { return BadRequest("Tag cannot be null or empty."); }
                var cats = await _dataService.GetManyCatsAsync(tag, page, pageSize);
                if (cats == null || !cats.Any()) { return NotFound($"No cats found with tag: {tag}"); }
                return Ok(cats);
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
                if (id <= 0) return BadRequest("Invalid cat ID.");
                var catImage = await _dataService.GetCatImageBase64Async(id);
                if(string.IsNullOrEmpty(catImage)) return NotFound($"Cat image with id: {id} was not found.");
                return File(ApiHelper.MapBase64toArray(catImage), "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cat image.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
