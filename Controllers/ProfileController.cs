using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomExpenseTracker.Data;
using RoomExpenseTracker.Models;
using RoomExpenseTracker.ViewModels;
using System.Security.Claims;

namespace RoomExpenseTracker.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _db;
    public ProfileController(ApplicationDbContext db) => _db = db;

    private string CurrentUserId   => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string CurrentUserName => User.FindFirstValue(ClaimTypes.Name)!;

    public async Task<IActionResult> Index()
    {
        var user    = await _db.Users.FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == CurrentUserId);
        var expenses = await _db.Expenses.Where(e => e.UserId == CurrentUserId).ToListAsync();

        if (profile == null)
        {
            profile = new UserProfile { UserId = CurrentUserId };
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }

        return View(new ProfileViewModel
        {
            UserId       = CurrentUserId,
            Name         = user?.Name ?? CurrentUserName,
            AvatarEmoji  = profile.AvatarEmoji,
            AvatarColor  = profile.AvatarColor,
            Bio          = profile.Bio,
            TotalExpenses= expenses.Sum(e => e.Amount),
            ExpenseCount = expenses.Count,
            Role         = user?.Role ?? "user"
        });
    }

    [HttpPost]
    public async Task<IActionResult> Update(string avatarEmoji, string avatarColor, string? bio)
    {
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == CurrentUserId);
        if (profile == null)
        {
            profile = new UserProfile { UserId = CurrentUserId };
            _db.UserProfiles.Add(profile);
        }
        profile.AvatarEmoji = avatarEmoji;
        profile.AvatarColor = avatarColor;
        profile.Bio         = bio;
        profile.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Profile update ho gaya!";
        return RedirectToAction("Index");
    }
}
