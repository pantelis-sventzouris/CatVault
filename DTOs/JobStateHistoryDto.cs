namespace CatVault.DTOs
{
    public class JobStateHistoryDto
    {
        public string State { get; set; }
        public string Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
