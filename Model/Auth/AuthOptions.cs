namespace LoginWebApp.Api.Model.Auth
{
    /// <summary>
    /// Which system issues the access tokens this API accepts
    /// </summary>
    public enum AuthProvider
    {
        /// <summary>Users stored in the local SQL database; this API issues its own JWTs</summary>
        Local,

        /// <summary>Users stored in an AWS Cognito user pool; Cognito issues the JWTs</summary>
        Cognito
    }

    /// <summary>
    /// Bound from the "Auth" configuration section
    /// </summary>
    public class AuthOptions
    {
        public const string SectionName = "Auth";

        public AuthProvider Provider { get; set; } = AuthProvider.Cognito;
        public LocalAuthOptions Local { get; set; } = new();
        public CognitoAuthOptions Cognito { get; set; } = new();
    }

    public class LocalAuthOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// HMAC key used to sign tokens. Keep it in user secrets, never in appsettings.
        /// </summary>
        public string SigningKey { get; set; } = string.Empty;

        public int TokenLifetimeMinutes { get; set; } = 60;
    }

    public class CognitoAuthOptions
    {
        public string Region { get; set; } = string.Empty;
        public string UserPoolId { get; set; } = string.Empty;

        /// <summary>
        /// App client id; access tokens issued to other clients of the pool are rejected
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Only set if the app client was created with a secret. Keep it in user secrets or the environment.
        /// </summary>
        public string? ClientSecret { get; set; }

        public string Authority => $"https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}";
    }
}
