using LoginWebApp.Api.Model.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoginWebApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns the caller's identity from their access token. Works with both local and Cognito tokens.
        /// </summary>
        [HttpGet("me")]
        public IActionResult Me()
        {
            var result = new
            {
                UserId = User.FindFirst(AuthClaimTypes.UserId)?.Value,
                Username = User.Identity?.Name,
                Groups = User.FindAll(AuthClaimTypes.Groups).Select(c => c.Value)
            };
            _logger.LogInformation("User info retrieved for {Username}", result.Username);
            return Ok(result);
        }
    }
}
