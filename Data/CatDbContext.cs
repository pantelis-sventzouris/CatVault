using Microsoft.EntityFrameworkCore;

public class CatDbContext : DbContext
{
    public DbSet<CatEntity> Cats { get; set; }
    public DbSet<TagEntity> Tags { get; set; }
    public DbSet<CatTag> CatTags { get; set; }

    public CatDbContext(DbContextOptions<CatDbContext> opts) : base(opts) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<CatTag>()
          .HasKey(ct => new { ct.CatEntityId, ct.TagEntityId });
        mb.Entity<CatTag>()
          .HasOne(ct => ct.CatEntity)
          .WithMany(c => c.CatTags)
          .HasForeignKey(ct => ct.CatEntityId);
        mb.Entity<CatTag>()
          .HasOne(ct => ct.TagEntity)
          .WithMany(t => t.CatTags)
          .HasForeignKey(ct => ct.TagEntityId);
    }
}
