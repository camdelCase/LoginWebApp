using LoginWebApp.Model.Auth;

namespace LoginWebApp.Services
{
    /// <summary>
    /// Login and registration for whichever provider Auth:Provider selects.
    /// AuthController depends only on this, so it works unchanged under both providers.
    /// </summary>
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<RegisterResult> RegisterAsync(RegisterRequest request);
        Task<ConfirmResult> ConfirmRegistrationAsync(ConfirmRegistrationRequest request);
    }
}
