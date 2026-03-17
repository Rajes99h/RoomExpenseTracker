using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomExpenseTracker.Data;
using RoomExpenseTracker.Services;
using RoomExpenseTracker.ViewModels;
using System.Security.Claims;

namespace RoomExpenseTracker.Controllers;

[Authorize]
public class ReportController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ExpenseService _svc;
    public ReportController(ApplicationDbContext db, ExpenseService svc) { _db = db; _svc = svc; }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index(int? year, int? month)
    {
        var now = DateTime.Today;
        var selectedYear  = year  ?? now.Year;
        var selectedMonth = month ?? now.Month;
        var from = new DateTime(selectedYear, selectedMonth, 1);
        var to   = from.AddMonths(1).AddDays(-1);

        var summary  = await _svc.GetSummaryAsync(CurrentUserId, from, to);
        var profiles = await _db.UserProfiles.ToListAsync();
        var avatars  = profiles.ToDictionary(p => p.UserId, p => p.AvatarEmoji);
        var colors   = profiles.ToDictionary(p => p.UserId, p => p.AvatarColor);

        var allExpenses = await _db.Expenses.ToListAsync();
        var availableMonths = allExpenses
            .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
            .Select(g => new MonthOption {
                Year = g.Key.Year, Month = g.Key.Month,
                Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy")
            })
            .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
            .Take(12).ToList();

        if (!availableMonths.Any(m => m.Year == now.Year && m.Month == now.Month))
            availableMonths.Insert(0, new MonthOption { Year = now.Year, Month = now.Month, Label = now.ToString("MMMM yyyy") });

        return View(new MonthlyReportViewModel {
            Year = selectedYear, Month = selectedMonth,
            MonthName = from.ToString("MMMM yyyy"),
            GrandTotal = summary.GrandTotal, TotalExpenses = summary.TotalExpenses,
            MyTotal = summary.MyTotal, FairShare = summary.FairShare, MyBalance = summary.MyBalance,
            UserSummaries = summary.UserSummaries,
            CategoryBreakdown = summary.CategoryBreakdown,
            Settlements = summary.Settlements,
            AvailableMonths = availableMonths,
            UserAvatars = avatars, UserColors = colors,
            FilterFrom = from, FilterTo = to
        });
    }
}
