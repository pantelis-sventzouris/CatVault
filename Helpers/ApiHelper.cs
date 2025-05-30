using CatVault.DTOs;

namespace CatVault.Helpers
{
    public static class ApiHelper
    {
        public static CatDto MapToDto(CatEntity c) => new CatDto
        {
            Id = c.Id,
            CatId = c.CatId,
            Width = c.Width,
            Height = c.Height,
            Created = c.Created,
            Tags = c.CatTags.Select(ct => ct.TagEntity.Name).ToList(),
            ImageBase64 = Convert.ToBase64String(c.Image)
        };

        public static byte[] MapBase64toArray(string base64)
        {
            return Convert.FromBase64String(base64);
        }
    }
}
