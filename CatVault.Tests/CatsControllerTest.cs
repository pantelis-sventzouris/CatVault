using CatVault.Controllers;
using CatVault.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatVault.CatVault.Tests
{
    public class CatsControllerTest
    {
        private CatDbContext GetDbWithCats()
        {
            var opts = new DbContextOptionsBuilder<CatDbContext>()
                .UseInMemoryDatabase("CatsCtl_" + Guid.NewGuid())
                .Options;
            var db = new CatDbContext(opts);

            var c1 = new CatEntity
            {
                CatId = "X1",
                Width = 10,
                Height = 10,
                Image = Array.Empty<byte>(),
                Created = DateTime.UtcNow,
                CatTags = new List<CatTag>
            {
                new CatTag { TagEntity = new TagEntity { Name = "Calm", Created = DateTime.UtcNow } }
            }
            };
            var c2 = new CatEntity
            {
                CatId = "X2",
                Width = 20,
                Height = 20,
                Image = Array.Empty<byte>(),
                Created = DateTime.UtcNow,
                CatTags = new List<CatTag>
            {
                new CatTag { TagEntity = new TagEntity { Name = "Playful", Created = DateTime.UtcNow } }
            }
            };
            db.Cats.AddRange(c1, c2);
            db.SaveChanges();
            return db;
        }
        [Fact]
        public async Task GetAll_ReturnsPagedList()
        {
            // Arrange
            var db = GetDbWithCats();
            var ctrl = new CatsController(null, null, null, db);

            // Act
            var actionResult = await ctrl.GetAll(tag: null, page: 1, pageSize: 1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var list = Assert.IsAssignableFrom<List<CatDto>>(ok.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task GetAll_FilterByTag_ReturnsOnlyMatching()
        {
            var db = GetDbWithCats();
            var ctrl = new CatsController(null, null, null, db);

            var result = await ctrl.GetAll(tag: "Playful", page: 1, pageSize: 10);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<CatDto>>(ok.Value);
            Assert.Single(list);
            Assert.Equal("X2", list[0].CatId);
        }

    }
}
