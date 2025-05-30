
using CatVault.DTOs;

namespace CatVault.Services
{
    public interface IDataService
    {
        Task<CatDto?> GetCatByIdAsync(int id);
        Task<List<CatDto>> GetManyCatsAsync(string tag, int page, int pageSize);
        Task<string> GetCatImageBase64Async(int id);

    }
}
