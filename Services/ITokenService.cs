using LoginWebApp.Api.Model;
using LoginWebApp.Api.Model.Auth;

namespace LoginWebApp.Api.Services
{
    public interface ITokenService
    {
        LoginResponse CreateAccessToken(User user);
    }
}
