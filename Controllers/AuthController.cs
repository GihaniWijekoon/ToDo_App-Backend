using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userService.RegisterAsync(request.Username, request.Password);
                return Ok(new { message = "User registered successfully", user });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration failed: {ex.Message}");
                return BadRequest(new { error = "Registration failed", details = ex.Message });
            }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var token = await _userService.AuthenticateAsync(request.Username, request.Password);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Invalid username or password attempt.");
                return Unauthorized(new { error = "Invalid username or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login failed: {ex.Message}");
                return BadRequest(new { error = "Login failed", details = ex.Message });
            }
        }
    }

 
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
