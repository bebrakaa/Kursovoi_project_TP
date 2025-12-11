using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InsuranceAgency.Application.Common.Security;
using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InsuranceAgency.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthController(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        /// <summary>
        /// Роль пользователя: Admin, Agent, Client. По умолчанию Client.
        /// </summary>
        public string Role { get; set; } = "Client";
    }

    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Все поля обязательны для заполнения" });
            }

            var existingByUsername = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingByUsername != null)
            {
                return BadRequest(new { message = "Пользователь с таким логином уже существует" });
            }

            var existingByEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingByEmail != null)
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            {
                return BadRequest(new { message = "Некорректная роль. Допустимо: Administrator, Agent, Client" });
            }

            var passwordHash = PasswordHasher.Hash(request.Password);
            var user = new User(request.Username, request.Email, role, passwordHash);
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return Ok(new { message = "Регистрация прошла успешно" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Ошибка при регистрации: {ex.Message}" });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Логин и пароль обязательны" });
            }

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }

            var token = GenerateJwt(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role.ToString()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Ошибка при входе: {ex.Message}" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            Role = user.Role.ToString()
        });
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            user.UpdateEmail(request.Email);
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            Role = user.Role.ToString()
        });
    }

    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> DeleteMe()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        await _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();

        return NoContent();
    }

    public class UpdateProfileRequest
    {
        public string? Email { get; set; }
    }

    private string GenerateJwt(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key") ?? "VeryStrongDevelopmentKey_ChangeInProduction_12345";
        var issuer = jwtSection.GetValue<string>("Issuer") ?? "InsuranceAgency";
        var audience = jwtSection.GetValue<string>("Audience") ?? "InsuranceAgencyClient";
        var expiresMinutes = jwtSection.GetValue<int?>("ExpiresMinutes") ?? 60;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


