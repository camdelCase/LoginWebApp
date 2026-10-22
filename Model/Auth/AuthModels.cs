using System.ComponentModel.DataAnnotations;

namespace LoginWebApp.Model.Auth
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required]
        [StringLength(128, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Cognito sends the confirmation code here
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }

    public class ConfirmRegistrationRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        /// <summary>
        /// True when the user must submit the emailed code to /confirm before logging in
        /// </summary>
        public bool ConfirmationRequired { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
