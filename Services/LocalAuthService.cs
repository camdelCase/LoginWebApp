using LoginWebApp.Model.Auth;

namespace LoginWebApp.Services
{
    /// <summary>
    /// Users in the local SQL database; tokens issued by this API. Only registered when Auth:Provider is Local.
    /// </summary>
    public class LocalAuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public LocalAuthService(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var user = await _userService.ValidateCredentialsAsync(request.Username, request.Password);
            return user is null
                ? LoginResult.Failed(LoginStatus.InvalidCredentials)
                : LoginResult.Success(_tokenService.CreateAccessToken(user));
        }

        public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
        {
            var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password);
            return new RegisterResult { Status = user is null ? RegisterStatus.UsernameTaken : RegisterStatus.Created };
        }

        public Task<ConfirmResult> ConfirmRegistrationAsync(ConfirmRegistrationRequest request) =>
            Task.FromResult(new ConfirmResult
            {
                Status = ConfirmStatus.NotRequired,
                Message = "Local users are active as soon as they register."
            });
    }
}
