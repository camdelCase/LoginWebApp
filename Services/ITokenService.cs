using LoginWebApp.Model;
using LoginWebApp.Model.Auth;

namespace LoginWebApp.Services
{
    public interface ITokenService
    {
        LoginResponse CreateAccessToken(User user);
    }
}
