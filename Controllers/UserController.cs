using LoginWebApp.Model.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoginWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Returns the caller's identity from their access token. Works with both local and Cognito tokens.
        /// </summary>
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                UserId = User.FindFirst(AuthClaimTypes.UserId)?.Value,
                Username = User.Identity?.Name,
                Groups = User.FindAll(AuthClaimTypes.Groups).Select(c => c.Value)
            });
        }
    }
}
