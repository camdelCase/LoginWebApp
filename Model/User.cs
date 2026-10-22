namespace LoginWebApp.Model
{
    /// <summary>
    /// A locally stored user. Only used when Auth:Provider is Local; under Cognito the
    /// user pool owns user records and the "sub" claim identifies the user.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Null only for users created before email was collected
        /// </summary>
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }
}
