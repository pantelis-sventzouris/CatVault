namespace CatVault.DTOs
{
    public class JobStatusDto
    {
        public string JobId { get; set; }
        public string State { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<JobStateHistoryDto> History { get; set; }
    }
}
