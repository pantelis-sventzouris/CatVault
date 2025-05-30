using CatVault.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Xunit;


namespace CatVault.CatVault.Tests
{
    public class CatFetchServiceTest
    {
        private CatDbContext GetInMemoryDb()
        {
            var opts = new DbContextOptionsBuilder<CatDbContext>()
                .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
                .Options;
            return new CatDbContext(opts);
        }

        [Fact]
        public async Task FetchAndStoreAsync_AddsNewCats_AndSkipsDuplicates()
        {
            // A fake HTTP handler returning 2 cats, one duplicate
            var fakeCats = new[]
            {
            new { id = "A", url = "http://test.gr/A.jpg", width = 100, height = 80, breeds = new[] { new { temperament = "Calm" } } },
            new { id = "B", url = "http://test.gr/B.jpg", width = 200, height = 150, breeds = new[] { new { temperament = "Playful,Friendly" } } }
        };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
              .Protected()
              .Setup<Task<HttpResponseMessage>>("SendAsync",
                 ItExpr.IsAny<HttpRequestMessage>(),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
              {
                  if (req.RequestUri.AbsolutePath.Contains("/search"))
                      return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(fakeCats) };
                  if (req.RequestUri.AbsoluteUri.EndsWith("A.jpg") || req.RequestUri.AbsoluteUri.EndsWith("B.jpg"))
                      return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[] { 0x1, 0x2 }) };
                  return new HttpResponseMessage(HttpStatusCode.NotFound);
              });

            var http = new HttpClient(handlerMock.Object);
            var db = GetInMemoryDb();
            var svc = new CatFetchService(db, http);

            // Seed one duplicate (id="A")
            db.Cats.Add(new CatEntity { CatId = "A", Width = 1, Height = 1, Image = new byte[] { }, Created = DateTime.UtcNow });
            await db.SaveChangesAsync();

            // Act
            var result = await svc.FetchAndStoreAsync("live_XeBVdyIPUbmgKPWHKMVPRafc7lnz4rCxhFDhUrwYrSQ7nNCJcKmpM0kpXnc8HBJj", "https://api.thecatapi.com/v1/images/search?limit=25&has_breeds=1", 25);

            // Assert: only B was added
            Assert.Single(result);
            Assert.Equal("B", result.First().CatId);

            // Tags: "Playful" and "Friendly"
            var tags = db.Tags.Select(t => t.Name).ToList();
            Assert.Contains("Playful", tags);
            Assert.Contains("Friendly", tags);
        }
    }
}
