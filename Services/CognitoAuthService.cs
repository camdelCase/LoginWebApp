using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using LoginWebApp.Model.Auth;
using Microsoft.Extensions.Options;

namespace LoginWebApp.Services
{
    /// <summary>
    /// Forwards login and registration to a Cognito user pool. Only registered when Auth:Provider is Cognito.
    /// Requires the app client to allow the USER_PASSWORD_AUTH flow (ALLOW_USER_PASSWORD_AUTH).
    /// </summary>
    public class CognitoAuthService : IAuthService
    {
        private readonly IAmazonCognitoIdentityProvider _cognito;
        private readonly CognitoAuthOptions _options;
        private readonly ILogger<CognitoAuthService> _logger;

        public CognitoAuthService(IAmazonCognitoIdentityProvider cognito, IOptions<AuthOptions> options, ILogger<CognitoAuthService> logger)
        {
            _cognito = cognito;
            _options = options.Value.Cognito;
            _logger = logger;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var authParameters = new Dictionary<string, string>
            {
                ["USERNAME"] = request.Username,
                ["PASSWORD"] = request.Password
            };
            var secretHash = ComputeSecretHash(request.Username);
            if (secretHash is not null)
            {
                authParameters["SECRET_HASH"] = secretHash;
            }

            try
            {
                var response = await _cognito.InitiateAuthAsync(new InitiateAuthRequest
                {
                    AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                    ClientId = _options.ClientId,
                    AuthParameters = authParameters
                });

                if (response.AuthenticationResult is null)
                {
                    _logger.LogInformation("Cognito returned challenge {Challenge} for {Username}", response.ChallengeName?.Value, request.Username);
                    return LoginResult.Failed(LoginStatus.ChallengeRequired, response.ChallengeName?.Value);
                }

                var result = response.AuthenticationResult;
                return LoginResult.Success(new LoginResponse
                {
                    AccessToken = result.AccessToken,
                    ExpiresAtUtc = DateTime.UtcNow.AddSeconds(result.ExpiresIn ?? 3600)
                });
            }
            catch (Exception ex) when (ex is NotAuthorizedException or UserNotFoundException)
            {
                return LoginResult.Failed(LoginStatus.InvalidCredentials);
            }
            catch (UserNotConfirmedException)
            {
                return LoginResult.Failed(LoginStatus.NotConfirmed);
            }
            catch (PasswordResetRequiredException)
            {
                return LoginResult.Failed(LoginStatus.ChallengeRequired, "PASSWORD_RESET_REQUIRED");
            }
        }

        public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _cognito.SignUpAsync(new SignUpRequest
                {
                    ClientId = _options.ClientId,
                    Username = request.Username,
                    Password = request.Password,
                    SecretHash = ComputeSecretHash(request.Username),
                    UserAttributes = [new AttributeType { Name = "email", Value = request.Email }]
                });
                return new RegisterResult
                {
                    Status = response.UserConfirmed == true ? RegisterStatus.Created : RegisterStatus.PendingConfirmation
                };
            }
            catch (UsernameExistsException)
            {
                return new RegisterResult { Status = RegisterStatus.UsernameTaken };
            }
            catch (Exception ex) when (ex is InvalidPasswordException or InvalidParameterException)
            {
                // Cognito's messages here describe what the user needs to fix, e.g. the password policy
                return new RegisterResult { Status = RegisterStatus.Invalid, Message = ex.Message };
            }
        }

        public async Task<ConfirmResult> ConfirmRegistrationAsync(ConfirmRegistrationRequest request)
        {
            try
            {
                await _cognito.ConfirmSignUpAsync(new ConfirmSignUpRequest
                {
                    ClientId = _options.ClientId,
                    Username = request.Username,
                    ConfirmationCode = request.Code,
                    SecretHash = ComputeSecretHash(request.Username)
                });
                return new ConfirmResult { Status = ConfirmStatus.Confirmed };
            }
            catch (Exception ex) when (ex is CodeMismatchException or ExpiredCodeException or UserNotFoundException or NotAuthorizedException)
            {
                return new ConfirmResult { Status = ConfirmStatus.InvalidCode, Message = "The code is invalid or has expired." };
            }
        }

        /// <summary>
        /// App clients created with a client secret require Base64(HMAC-SHA256(secret, username + clientId)) on every call
        /// </summary>
        private string? ComputeSecretHash(string username)
        {
            if (string.IsNullOrEmpty(_options.ClientSecret))
            {
                return null;
            }

            var hash = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(_options.ClientSecret),
                Encoding.UTF8.GetBytes(username + _options.ClientId));
            return Convert.ToBase64String(hash);
        }
    }
}
