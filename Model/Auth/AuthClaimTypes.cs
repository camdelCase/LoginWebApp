namespace LoginWebApp.Api.Model.Auth
{
    /// <summary>
    /// Claim names used by Cognito access tokens. Local tokens use the same names so
    /// code that reads claims works unchanged under either provider.
    /// </summary>
    public static class AuthClaimTypes
    {
        public const string UserId = "sub";
        public const string Username = "username";
        public const string Groups = "cognito:groups";
        public const string TokenUse = "token_use";
        public const string ClientId = "client_id";
    }
}
