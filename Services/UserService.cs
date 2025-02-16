// Services/UserService.cs
using Backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserService(IConfiguration configuration, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<User> RegisterAsync(string username, string password)
        {
            var user = new User
            {
                UserName = username,
                FirstName = "DefaultFirstName",
                LastName = "DefaultLastName",
                Address = "DefaultAddress",
                PhoneNumber = "1234567890"
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"User registration failed: {errors}");
            }

            return user;
        }



        public async Task<string> AuthenticateAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
