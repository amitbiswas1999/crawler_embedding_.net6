
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VectorController : ControllerBase
    {
        [HttpGet("{ids}")]
        public async Task<ActionResult<string>> Get(string ids)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new System.Uri($"https://test1-5a3ac21.svc.us-west1-gcp-free.pinecone.io/vectors/fetch?ids={ids}"),
                Headers =
                {
                    { "accept", "application/json" },
                    { "Api-Key", "76459ca8-165f-492f-a052-65c9c85071b8" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return body;
            }
        }
    }
}


















