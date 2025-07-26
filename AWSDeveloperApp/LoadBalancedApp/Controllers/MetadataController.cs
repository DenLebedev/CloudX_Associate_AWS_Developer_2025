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
            try
            {
                // Request IMDSv2 token
                var tokenRequest = new HttpRequestMessage(HttpMethod.Put, "http://169.254.169.254/latest/api/token");
                tokenRequest.Headers.Add("X-aws-ec2-metadata-token-ttl-seconds", "21600");
                var tokenResponse = await httpClient.SendAsync(tokenRequest);
                tokenResponse.EnsureSuccessStatusCode();
                var token = await tokenResponse.Content.ReadAsStringAsync();

                // Request AZ using token
                var azRequest = new HttpRequestMessage(HttpMethod.Get, "http://169.254.169.254/latest/meta-data/placement/availability-zone");
                azRequest.Headers.Add("X-aws-ec2-metadata-token", token);
                var azResponse = await httpClient.SendAsync(azRequest);
                azResponse.EnsureSuccessStatusCode();
                var az = await azResponse.Content.ReadAsStringAsync();

                var region = az[..^1]; // Remove the AZ suffix (e.g. "a") to get the region

                return Ok(new { region, availabilityZone = az });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to access EC2 metadata", details = ex.Message });
            }
        }
    }
}

