using Microsoft.AspNetCore.Mvc;

namespace LoadBalancedApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetadataController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient();

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (environment == "Development")
            {
                return Ok(new { region = "us-east-1", availabilityZone = "us-east-1a (mock)" });
            }

            try
            {
                string az = await httpClient.GetStringAsync("http://169.254.169.254/latest/meta-data/placement/availability-zone");
                string region = az[..^1]; // remove last character
                return Ok(new { region, availabilityZone = az });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = "Failed to access EC2 metadata", details = ex.Message });
            }
        }
    }
}

