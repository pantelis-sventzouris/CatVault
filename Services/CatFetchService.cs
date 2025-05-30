using CatVault.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CatVault.Services
{
    public class CatFetchService : ICatFetchService
    {
        private readonly CatDbContext _db;
        private readonly HttpClient _http;

        public CatFetchService(CatDbContext db, HttpClient http)
        {
            _db = db;
            _http = http;
        }

        public async Task<IEnumerable<CatEntity>> FetchAndStoreAsync(string apiKey, string apiUrl, int count)
        {
            _http.DefaultRequestHeaders.Add("x-api-key", apiKey);
            string url = $"{apiUrl}{count}&has_breeds=1";
            var response = await _http.GetFromJsonAsync<List<CatApiDto>>(url);
            var stored = new List<CatEntity>();

            foreach (var dto in response)
            {
                if (await _db.Cats.AnyAsync(c => c.CatId == dto.Id)) { continue; }
                var imageBytes = await _http.GetByteArrayAsync(dto.Url);
                var cat = new CatEntity
                {
                    CatId = dto.Id,
                    Width = dto.Width,
                    Height = dto.Height,
                    Image = imageBytes,
                    Created = DateTime.UtcNow
                };
                _db.Cats.Add(cat);
                var temperaments = dto.Breeds.SelectMany(b => b.Temperament.Split(',', StringSplitOptions.TrimEntries));
                foreach (var temp in temperaments)
                {
                    var tag = await _db.Tags.SingleOrDefaultAsync(t => t.Name == temp)
                           ?? _db.Tags.Add(new TagEntity { Name = temp, Created = DateTime.UtcNow }).Entity;

                    _db.CatTags.Add(new CatTag
                    {
                        CatEntity = cat,
                        TagEntity = tag
                    });
                }

                stored.Add(cat);
            }

            await _db.SaveChangesAsync();
            return stored;
        }
    }
}
