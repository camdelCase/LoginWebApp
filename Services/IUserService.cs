using LoginWebApp.Model;

namespace LoginWebApp.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Returns the user if the username and password match, otherwise null
        /// </summary>
        Task<User?> ValidateCredentialsAsync(string username, string password);

        /// <summary>
        /// Creates a user, or returns null if the username is already taken
        /// </summary>
        Task<User?> RegisterAsync(string username, string email, string password);
    }
}
