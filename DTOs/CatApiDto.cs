namespace CatVault.DTOs
{
    public class CatApiDto
    {
        public string Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Url { get; set; }
        public List<BreedDto> Breeds { get; set; }
    }
}
