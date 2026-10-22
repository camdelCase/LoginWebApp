using System.Security.Claims;
using System.Text;
using LoginWebApp.Api.Model;
using LoginWebApp.Api.Model.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LoginWebApp.Api.Services
{
    /// <summary>
    /// Issues JWTs for locally stored users. Only registered when Auth:Provider is Local.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly LocalAuthOptions _options;
        private readonly SigningCredentials _signingCredentials;
        private readonly JsonWebTokenHandler _tokenHandler = new();

        public TokenService(IOptions<AuthOptions> options)
        {
            _options = options.Value.Local;
            _signingCredentials = new SigningCredentials(
                CreateSigningKey(_options.SigningKey), SecurityAlgorithms.HmacSha256);
        }

        public LoginResponse CreateAccessToken(User user)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_options.TokenLifetimeMinutes);

            var token = _tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Expires = expiresAt,
                SigningCredentials = _signingCredentials,
                Subject = new ClaimsIdentity(
                [
                    new Claim(AuthClaimTypes.UserId, user.Id.ToString()),
                    new Claim(AuthClaimTypes.Username, user.Username),
                    new Claim(AuthClaimTypes.TokenUse, "access")
                ])
            });

            return new LoginResponse { AccessToken = token, ExpiresAtUtc = expiresAt };
        }

        public static SymmetricSecurityKey CreateSigningKey(string signingKey) =>
            new(Encoding.UTF8.GetBytes(signingKey));
    }
}
