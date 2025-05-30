using CatVault.Controllers;
using CatVault.DTOs;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CatVault.CatVault.Tests
{
    public class JobsControllerTest
    {
        [Fact]
        public void GetStatus_ReturnsNotFound_ForUnknownJob()
        {
            // Arrange
            var mockApi = new Mock<IMonitoringApi>();
            mockApi.Setup(m => m.JobDetails("nope")).Returns((JobDetailsDto)null);

            var ctrl = new JobsController(null);

            // Act
            var result = ctrl.GetJobStatus("nope");

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetStatus_ReturnsHistory_ForKnownJob()
        {
            // Arrange
            var historyList = new List<StateHistoryDto>
        {
            new StateHistoryDto{ CreatedAt = DateTime.Now.AddMinutes(-1) },
            new StateHistoryDto { CreatedAt = DateTime.Now.AddMinutes(2)}
        };
            var dto = new JobDetailsDto
            {
                CreatedAt = DateTime.UtcNow.AddMinutes(-2),
                History = historyList
            };

            var mockApi = new Mock<IMonitoringApi>();
            mockApi.Setup(m => m.JobDetails("42")).Returns(dto);

            var ctrl = new JobsController(null);

            // Act
            var result = ctrl.GetJobStatus("42");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var body = Assert.IsType<JobStatusDto>(ok.Value);
            Assert.Equal("42", body.JobId);
            Assert.Equal("Processing", body.State);
            Assert.Equal(2, body.History.Count);
        }
    }
}
