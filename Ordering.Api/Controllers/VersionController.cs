using Microsoft.AspNetCore.Mvc;

namespace Ordering.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public object Get()
        {
            return new { 
                version = "1.0.0",
                timestamp = DateTime.UtcNow,
                message = "Version endpoint works!"
            };
        }
    }
}
