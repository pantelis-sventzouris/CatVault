public class CatEntity
{
    public int Id { get; set; }
    public string CatId { get; set; }          
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[] Image { get; set; }           
    public DateTime Created { get; set; }

    public ICollection<CatTag> CatTags { get; set; }
}
