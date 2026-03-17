using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomExpenseTracker.Data;
using RoomExpenseTracker.ViewModels;
using System.Security.Claims;

namespace RoomExpenseTracker.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;
    public AccountController(ApplicationDbContext db) => _db = db;

    [HttpGet] public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _db.Users.FirstOrDefaultAsync(u => 
            (u.UserId == vm.UserId || u.Name == vm.UserId) && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(vm.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View(vm);
        }
        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role)
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });
        return RedirectToAction("Index", "Expenses");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var uid  = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == uid);
        if (user == null || !BCrypt.Net.BCrypt.Verify(vm.OldPassword, user.PasswordHash))
        {
            ModelState.AddModelError("OldPassword", "Current password is incorrect.");
            return View(vm);
        }
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Password updated successfully!";
        return RedirectToAction("ChangePassword");
    }
}
