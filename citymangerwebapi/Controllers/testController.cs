using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace citymangerwebapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class testController : ControllerBase
    {
        [HttpGet]

        public string Method()
        {
            return "hello world";
        }
    }
}
