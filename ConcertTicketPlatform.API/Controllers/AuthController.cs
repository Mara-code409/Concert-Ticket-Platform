using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConcertTicketPlatform.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ConcertTicketPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<AppUser> userManager, IConfiguration config, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _config      = config;
            _logger      = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Email și parola sunt obligatorii." });

            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return BadRequest(new { message = "Un cont cu acest email există deja." });

            var user = new AppUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation("New user registered: {Email}", dto.Email);

            var token = await GenerateJwtToken(user);
            return Ok(new { token, email = user.Email, fullName = user.FullName });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
                return Unauthorized(new { message = "Email sau parolă incorectă." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtToken(user);

            _logger.LogInformation("User logged in: {Email}", dto.Email);
            return Ok(new { token, email = user.Email, fullName = user.FullName, roles });
        }

        private async Task<string> GenerateJwtToken(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var expiry = int.TryParse(_config["Jwt:ExpiryDays"], out var days) ? days : 7;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub,   user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new(ClaimTypes.Name,               user.UserName!),
                new("fullName",                    user.FullName)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer:             _config["Jwt:Issuer"],
                audience:           _config["Jwt:Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.AddDays(expiry),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public record RegisterDto(string Email, string Password, string FullName);
    public record LoginDto(string Email, string Password);
}
