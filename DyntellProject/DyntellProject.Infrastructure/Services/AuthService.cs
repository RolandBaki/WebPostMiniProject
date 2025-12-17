using DyntellProject.Core.DTOs;
using DyntellProject.Core.Entities;
using DyntellProject.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DyntellProject.Infrastructure.Services;



public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }
    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
    {
        try
        {
            // Username keresese
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: User {Username} not found", loginDto.Username);
                return null;
            }

            // Jelszo check
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Login attempt failed: Invalid password for user {Username}", loginDto.Username);
                return null;
            }

            // Token generalas
            var token = GenerateJwtToken(user);

            _logger.LogInformation("User {Username} logged in successfully", loginDto.Username);

            // Valasz generalasa
            return new LoginResponseDto
            {
                Token = token,
                Username = user.UserName ?? string.Empty,
                Role = user.Role.ToString(),
                AgeGroup = user.AgeGroup.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", loginDto.Username);
            return null;
        }
    }

    public async Task<(LoginResponseDto? result, string[]? errors)> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // User letrehozasa
            var user = new User
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                AgeGroup = registerDto.AgeGroup,
                Role = UserRole.Standard
            };

            // Uj felhasznalo beszurasa
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            // Sikeresseg ellenorzese
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                _logger.LogWarning("Registration failed for user {Username}: {Errors}",
                    registerDto.Username, string.Join(", ", errors));
                return (null, errors);
            }

            _logger.LogInformation("User {Username} registered successfully", registerDto.Username);

            // Valasz szerkesztese
            var loginDto = new LoginDto
            {
                Username = registerDto.Username,
                Password = registerDto.Password
            };

            // Automatikus bejelentkezes regisztralas utan 
            var loginResult = await LoginAsync(loginDto);
            return (loginResult, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", registerDto.Username);
            return (null, new[] { ex.Message });
        }
    }

    private string GenerateJwtToken(User user)
    {
        // Claims a felhasznalorol
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("Role", user.Role.ToString()),
            new Claim("AgeGroup", user.AgeGroup.ToString())
        };

        // Titkos kulcs
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // TokenContext osszeallitas
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:DurationInMinutes"] ?? "30")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
