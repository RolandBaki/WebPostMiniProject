using DyntellProject.Core.DTOs;
using DyntellProject.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DyntellProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    { 
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register", Name = "PostRegister")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Bemenet ellenorzes
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration attempt failed - Invalid model state. Username: {Username}", registerDto.Username);
                return BadRequest(ModelState);
            }

            var (result, errors) = await _authService.RegisterAsync(registerDto);
            if (result == null)
            {
                _logger.LogWarning("Registration failed - Username: {Username}, Email: {Email}, Errors: {Errors}", 
                    registerDto.Username, registerDto.Email, string.Join(", ", errors ?? new[] { "Unknown error occurred." }));
                return BadRequest(new
                {
                    message = "Registration failed.",
                    errors = errors ?? new[] { "Unknown error occurred." }
                });
            }

            _logger.LogInformation("User registered successfully - Username: {Username}, Email: {Email}, AgeGroup: {AgeGroup}, Timestamp: {Timestamp}", 
                result.Username, registerDto.Email, registerDto.AgeGroup, DateTime.UtcNow);
            return Ok(result);
        }
        

        [HttpPost("login", Name = "PostLogin")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Bemenet ellenorzes
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login attempt failed - Invalid model state. Username: {Username}", loginDto.Username);
                return BadRequest(ModelState);
            }

            // bejelentkezes
            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                _logger.LogWarning("Login failed - Invalid credentials. Username: {Username}, Timestamp: {Timestamp}", 
                    loginDto.Username, DateTime.UtcNow);
                return Unauthorized(new { message = "Invalid username or password." });
            }

            _logger.LogInformation("User logged in successfully - Username: {Username}, Role: {Role}, AgeGroup: {AgeGroup}, Timestamp: {Timestamp}", 
                result.Username, result.Role, result.AgeGroup, DateTime.UtcNow);
            return Ok(result);
        }
        
    }
}
