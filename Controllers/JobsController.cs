using CatVault.DTOs;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;

namespace CatVault.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobsController : Controller
    {
        private readonly IMonitoringApi _monitoringApi;
        private readonly ILogger<JobsController> _logger;

        public JobsController(ILogger<JobsController> logger)
        {
            _monitoringApi = JobStorage.Current.GetMonitoringApi();
            _logger = logger;
        }

        [HttpGet("{jobId}")]
        public ActionResult<JobStatusDto> GetJobStatus(string jobId)
        {
            try
            {
                var jobDetails = _monitoringApi.JobDetails(jobId);
                if (jobDetails == null) return NotFound($"Job with id: {jobId} not found.");

                var currentState = jobDetails.History.FirstOrDefault()?.StateName;
                var createdAt = jobDetails.CreatedAt;

                var jobHistory = jobDetails.History
                    .Select(h => new JobStateHistoryDto
                    {
                        State = h.StateName,
                        Reason = h.Reason,
                        Timestamp = h.CreatedAt
                    })
                    .ToList();

                var result = new JobStatusDto
                {
                    JobId = jobId,
                    State = currentState,
                    CreatedAt = createdAt.Value,
                    History = jobHistory
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get job status for ID: {jobId}");
                return StatusCode(500, "Internal Server Error");
            }

        }


    }
}
