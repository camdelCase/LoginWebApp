using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LoginWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Basic health check to confirm the User API is running
        /// </summary>
        /// <returns></returns>
        [HttpGet("healthcheck")]
        public async Task<ActionResult> HealthCheck()
        {
            return Ok();
        }
    }
}
