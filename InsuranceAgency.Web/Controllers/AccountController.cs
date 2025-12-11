using System.Security.Claims;
using InsuranceAgency.Application.Common.Security;
using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAgency.Web.Controllers;

/// <summary>
/// MVC-контроллер для страниц регистрации и входа.
/// </summary>
public class AccountController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AccountController(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Логин и пароль обязательны";
            return View();
        }

        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        // Создаем claims для cookie authentication
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string username, string email, string password, string role)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || 
            string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Все поля обязательны для заполнения";
            return View();
        }

        var existingByUsername = await _userRepository.GetByUsernameAsync(username);
        if (existingByUsername != null)
        {
            ViewBag.Error = "Пользователь с таким логином уже существует";
            return View();
        }

        var existingByEmail = await _userRepository.GetByEmailAsync(email);
        if (existingByEmail != null)
        {
            ViewBag.Error = "Пользователь с таким email уже существует";
            return View();
        }

        if (!Enum.TryParse<UserRole>(role, true, out var userRole))
        {
            ViewBag.Error = "Некорректная роль";
            return View();
        }

        var passwordHash = PasswordHasher.Hash(password);
        var user = new User(username, email, userRole, passwordHash);
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Автоматически входим после регистрации
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}


