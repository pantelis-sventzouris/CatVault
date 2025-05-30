public class TagEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; }

    public ICollection<CatTag> CatTags { get; set; }
}
