// Services/IUserService.cs
using Backend.Models;

namespace Backend.Services
{
    public interface IUserService
    {
        Task<User> RegisterAsync(string username, string password);
        Task<string> AuthenticateAsync(string username, string password);
    }
}
