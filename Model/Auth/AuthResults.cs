namespace LoginWebApp.Model.Auth
{
    public enum LoginStatus
    {
        Success,

        /// <summary>Unknown user or wrong password; deliberately indistinguishable</summary>
        InvalidCredentials,

        /// <summary>Registered but the confirmation code has not been entered yet (Cognito only)</summary>
        NotConfirmed,

        /// <summary>Cognito wants another step (MFA, new password, password reset) that this API does not handle yet</summary>
        ChallengeRequired
    }

    public class LoginResult
    {
        public LoginStatus Status { get; init; }
        public LoginResponse? Tokens { get; init; }
        public string? ChallengeName { get; init; }

        public static LoginResult Success(LoginResponse tokens) => new() { Status = LoginStatus.Success, Tokens = tokens };
        public static LoginResult Failed(LoginStatus status, string? challengeName = null) => new() { Status = status, ChallengeName = challengeName };
    }

    public enum RegisterStatus
    {
        /// <summary>The user can log in now</summary>
        Created,

        /// <summary>The user must submit the code Cognito sent before logging in</summary>
        PendingConfirmation,

        UsernameTaken,

        /// <summary>Rejected by the provider's rules, e.g. Cognito's password policy; see Message</summary>
        Invalid
    }

    public class RegisterResult
    {
        public RegisterStatus Status { get; init; }
        public string? Message { get; init; }
    }

    public enum ConfirmStatus
    {
        Confirmed,
        InvalidCode,

        /// <summary>The provider has no confirmation step (local users are active immediately)</summary>
        NotRequired
    }

    public class ConfirmResult
    {
        public ConfirmStatus Status { get; init; }
        public string? Message { get; init; }
    }
}
