using LoginWebApp.Api.Model.Auth;
using LoginWebApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoginWebApp.Api.Controllers
{
    /// <summary>
    /// Login and registration for the app's own login page. Works with both providers:
    /// Local checks the SQL database, Cognito forwards each request to the user pool.
    /// </summary>
    [Route("api/user/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Exchanges a username and password for an access token
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            switch (result.Status)
            {
                case LoginStatus.Success:
                    return Ok(result.Tokens);

                case LoginStatus.NotConfirmed:
                    return Problem("Account not confirmed. Enter the code sent to your email.",
                        statusCode: StatusCodes.Status403Forbidden);

                case LoginStatus.ChallengeRequired:
                    return Problem($"Additional sign-in step required ({result.ChallengeName}), which is not supported yet.",
                        statusCode: StatusCodes.Status403Forbidden);

                default:
                    // Same response whether the user is unknown or the password is wrong
                    _logger.LogInformation("Failed login for {Username}", request.Username);
                    return Unauthorized();
            }
        }

        /// <summary>
        /// Creates a user. Under Cognito the user must then confirm with the emailed code before logging in.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            return result.Status switch
            {
                RegisterStatus.Created => Created((string?)null, new RegisterResponse { ConfirmationRequired = false }),
                RegisterStatus.PendingConfirmation => Created((string?)null, new RegisterResponse { ConfirmationRequired = true }),
                RegisterStatus.UsernameTaken => Conflict("Username is already taken"),
                _ => Problem(result.Message, statusCode: StatusCodes.Status400BadRequest)
            };
        }

        /// <summary>
        /// Confirms a new account with the code Cognito emailed to the user
        /// </summary>
        [HttpPost("confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Confirm(ConfirmRegistrationRequest request)
        {
            var result = await _authService.ConfirmRegistrationAsync(request);

            return result.Status == ConfirmStatus.Confirmed
                ? NoContent()
                : Problem(result.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
