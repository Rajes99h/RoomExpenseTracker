using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomExpenseTracker.Data;
using RoomExpenseTracker.Models;
using RoomExpenseTracker.ViewModels;
using System.Security.Claims;

namespace RoomExpenseTracker.Controllers;

[Authorize]
public class SabjiController : Controller
{
    private readonly ApplicationDbContext _db;
    public SabjiController(ApplicationDbContext db) => _db = db;

    private string CurrentUserId   => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string CurrentUserName => User.FindFirstValue(ClaimTypes.Name)!;
    private bool   IsAdmin         => User.IsInRole("admin");

    public async Task<IActionResult> Index()
    {
        var users    = await _db.Users.Where(u => u.IsActive).ToListAsync();
        var absences = await _db.Absences.ToListAsync();
        var turns    = await _db.SabjiTurns.OrderBy(t => t.WeekOrder).ToListAsync();
        var today    = DateTime.Today;

        // Get current week Monday
        var monday = today.AddDays(-(int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1);

        var weekSchedule = new List<DayTurnInfo>();

        for (int i = 0; i < 7; i++)
        {
            var date    = monday.AddDays(i);
            var dayNum  = (int)date.DayOfWeek; // 0=Sun,1=Mon...
            var dayName = date.ToString("dddd");

            // Get assigned user for this day
            var turn = turns.FirstOrDefault(t => t.DayOfWeek == dayNum);
            string? assignedUserId   = turn?.UserId;
            string? assignedUserName = users.FirstOrDefault(u => u.UserId == assignedUserId)?.Name;

            // Check if assigned user is absent
            bool isAbsent = false;
            if (assignedUserId != null)
            {
                isAbsent = absences.Any(a => a.UserId == assignedUserId &&
                    a.FromDate.Date <= date && a.ToDate.Date >= date);
            }

            // If absent, find next available user
            if (isAbsent && users.Count > 1)
            {
                var rotation = turns.OrderBy(t => t.WeekOrder).ToList();
                var currentIdx = rotation.FindIndex(t => t.UserId == assignedUserId);
                for (int j = 1; j < users.Count; j++)
                {
                    var nextIdx  = (currentIdx + j) % rotation.Count;
                    var nextUser = rotation.ElementAtOrDefault(nextIdx);
                    if (nextUser == null) break;
                    bool nextAbsent = absences.Any(a => a.UserId == nextUser.UserId &&
                        a.FromDate.Date <= date && a.ToDate.Date >= date);
                    if (!nextAbsent)
                    {
                        assignedUserId   = nextUser.UserId;
                        assignedUserName = users.FirstOrDefault(u => u.UserId == assignedUserId)?.Name;
                        break;
                    }
                }
            }

            // Check if purchased
            var purchase = await _db.SabjiPurchases
                .FirstOrDefaultAsync(p => p.PurchaseDate.Date == date);

            weekSchedule.Add(new DayTurnInfo
            {
                Date             = date,
                DayName          = dayName,
                AssignedUserId   = assignedUserId,
                AssignedUserName = assignedUserName,
                IsToday          = date == today,
                IsPurchased      = purchase != null,
                IsAbsent         = isAbsent,
                ActualBuyerName  = purchase?.UserName
            });
        }

        var vm = new SabjiTurnViewModel
        {
            WeekSchedule = weekSchedule,
            Today        = weekSchedule.FirstOrDefault(d => d.IsToday),
            AllUsers     = users,
            IsAdmin      = IsAdmin
        };

        return View(vm);
    }

    // Admin assigns a user to a day
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AssignTurn(int dayOfWeek, string userId)
    {
        var existing = await _db.SabjiTurns.FirstOrDefaultAsync(t => t.DayOfWeek == dayOfWeek);
        if (existing != null)
        {
            existing.UserId = userId;
        }
        else
        {
            var maxOrder = await _db.SabjiTurns.MaxAsync(t => (int?)t.WeekOrder) ?? 0;
            _db.SabjiTurns.Add(new SabjiTurn
            {
                UserId    = userId,
                DayOfWeek = dayOfWeek,
                WeekOrder = maxOrder + 1
            });
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Turn assigned!";
        return RedirectToAction("Index");
    }

    // Setup automatic rotation for whole week
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AutoSetup()
    {
        var users = await _db.Users.Where(u => u.IsActive).ToListAsync();
        if (!users.Any()) { TempData["Error"] = "No users found!"; return RedirectToAction("Index"); }

        // Remove existing turns
        var existing = await _db.SabjiTurns.ToListAsync();
        _db.SabjiTurns.RemoveRange(existing);

        // Assign users to days Mon-Sun (1-7) in rotation
        int[] days = { 1, 2, 3, 4, 5, 6, 0 }; // Mon to Sun
        for (int i = 0; i < days.Length; i++)
        {
            var user = users[i % users.Count];
            _db.SabjiTurns.Add(new SabjiTurn
            {
                UserId    = user.UserId,
                DayOfWeek = days[i],
                WeekOrder = i + 1
            });
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Auto rotation set for all 7 days!";
        return RedirectToAction("Index");
    }

    // Mark as purchased
    [HttpPost]
    public async Task<IActionResult> MarkPurchased()
    {
        var today    = DateTime.Today;
        var existing = await _db.SabjiPurchases
            .FirstOrDefaultAsync(p => p.PurchaseDate.Date == today);

        if (existing != null)
        {
            existing.UserId   = CurrentUserId;
            existing.UserName = CurrentUserName;
        }
        else
        {
            _db.SabjiPurchases.Add(new SabjiPurchase
            {
                UserId       = CurrentUserId,
                UserName     = CurrentUserName,
                PurchaseDate = today,
                Purchased    = true
            });
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Sabji kharidi mark ho gayi!";
        return RedirectToAction("Index");
    }

    // Admin updates who actually bought sabji on a specific date
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateBuyer(string date, string userId)
    {
        var purchaseDate = DateTime.Parse(date).Date;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null) { TempData["Error"] = "User not found!"; return RedirectToAction("Index"); }

        var existing = await _db.SabjiPurchases
            .FirstOrDefaultAsync(p => p.PurchaseDate.Date == purchaseDate);

        if (existing != null)
        {
            existing.UserId   = userId;
            existing.UserName = user.Name;
        }
        else
        {
            _db.SabjiPurchases.Add(new SabjiPurchase
            {
                UserId       = userId,
                UserName     = user.Name,
                PurchaseDate = purchaseDate,
                Purchased    = true
            });
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = $"{user.Name} ne {purchaseDate:dd MMM} ko sabji kharidi — updated!";
        return RedirectToAction("Index");
    }

    // Admin removes purchase mark for a date
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RemovePurchase(string date)
    {
        var purchaseDate = DateTime.Parse(date).Date;
        var existing = await _db.SabjiPurchases
            .FirstOrDefaultAsync(p => p.PurchaseDate.Date == purchaseDate);
        if (existing != null)
        {
            _db.SabjiPurchases.Remove(existing);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Purchase mark hata diya!";
        }
        return RedirectToAction("Index");
    }
}
