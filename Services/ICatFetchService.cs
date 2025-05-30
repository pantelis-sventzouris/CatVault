namespace CatVault.Services
{
    public interface ICatFetchService
    {
        Task<IEnumerable<CatEntity>> FetchAndStoreAsync(string apiKey, string apiUrl, int count);
    }
}
