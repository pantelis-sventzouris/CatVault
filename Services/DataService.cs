using CatVault.DTOs;
using CatVault.Helpers;
using Microsoft.EntityFrameworkCore;

namespace CatVault.Services
{
    public class DataService : IDataService
    {
        private readonly CatDbContext _db;
        public DataService(CatDbContext db)
        {
            _db = db;
        }
        public async Task<CatDto?> GetCatByIdAsync(int id)
        {
            var dbItem = await _db.Cats.Include(c => c.CatTags).ThenInclude(ct => ct.TagEntity).FirstOrDefaultAsync(c => c.Id == id);
            if (dbItem == null) return null;
            return ApiHelper.MapToDto(dbItem);
        }
        public async Task<List<CatDto>> GetManyCatsAsync(string tag, int page, int pageSize)
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

            return dtos;
        }

        public async Task<string> GetCatImageBase64Async(int id)
        {
            var img = await _db.Cats.Where(c => c.Id == id).Select(c => c.Image).FirstOrDefaultAsync();
            if (img == null) return string.Empty;
            return Convert.ToBase64String(img);
        }
    }
}
