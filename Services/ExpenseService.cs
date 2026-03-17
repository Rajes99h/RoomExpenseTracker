using Microsoft.EntityFrameworkCore;
using RoomExpenseTracker.Data;
using RoomExpenseTracker.Models;
using RoomExpenseTracker.ViewModels;

namespace RoomExpenseTracker.Services;

public class ExpenseService
{
    private readonly ApplicationDbContext _db;
    public ExpenseService(ApplicationDbContext db) => _db = db;

    public async Task<List<Expense>> GetUserExpensesAsync(string userId, DateTime? from, DateTime? to)
    {
        var q = _db.Expenses.Where(e => e.UserId == userId);
        if (from.HasValue) q = q.Where(e => e.ExpenseDate >= from.Value);
        if (to.HasValue)   q = q.Where(e => e.ExpenseDate <= to.Value);
        return await q.OrderByDescending(e => e.ExpenseDate).ToListAsync();
    }

    public async Task<List<Expense>> GetAllExpensesAsync(DateTime? from, DateTime? to, string? userId)
    {
        var q = _db.Expenses.AsQueryable();
        if (from.HasValue)  q = q.Where(e => e.ExpenseDate >= from.Value);
        if (to.HasValue)    q = q.Where(e => e.ExpenseDate <= to.Value);
        if (!string.IsNullOrEmpty(userId)) q = q.Where(e => e.UserId == userId);
        return await q.OrderByDescending(e => e.ExpenseDate).ToListAsync();
    }

    // Check if a user was absent on a specific date
    private bool IsUserAbsentOnDate(string userId, DateTime date, List<Absence> absences)
    {
        return absences.Any(a =>
            a.UserId == userId &&
            a.FromDate.Date <= date.Date &&
            a.ToDate.Date >= date.Date);
    }

    public async Task<SummaryViewModel> GetSummaryAsync(string currentUserId, DateTime? from, DateTime? to)
    {
        var allExpenses = await GetAllExpensesAsync(from, to, null);
        var users       = await _db.Users.Where(u => u.IsActive).ToListAsync();
        var absences    = await _db.Absences.ToListAsync();

        // ── CORE LOGIC ──────────────────────────────────────────────────────
        // For EACH expense, calculate fair share only among users who were
        // PRESENT on that expense date (not absent)
        // ────────────────────────────────────────────────────────────────────

        // Track per-user: how much they paid, and how much they owe (fair share)
        var paid      = users.ToDictionary(u => u.UserId, _ => 0m);
        var owes      = users.ToDictionary(u => u.UserId, _ => 0m);

        foreach (var expense in allExpenses)
        {
            // Who was PRESENT on this expense date?
            var presentUsers = users.Where(u =>
                !IsUserAbsentOnDate(u.UserId, expense.ExpenseDate, absences)
            ).ToList();

            // Record who paid
            if (paid.ContainsKey(expense.UserId))
                paid[expense.UserId] += expense.Amount;

            // Split equally among present users only
            if (presentUsers.Count > 0)
            {
                var share = expense.Amount / presentUsers.Count;
                foreach (var u in presentUsers)
                    owes[u.UserId] += share;
            }
        }

        var grandTotal = allExpenses.Sum(e => e.Amount);

        // Build user summaries
        var userSummaries = users.Select(u =>
        {
            var userPaid  = paid[u.UserId];
            var userOwes  = Math.Round(owes[u.UserId], 2);
            var balance   = Math.Round(userPaid - userOwes, 2);

            // Is user currently absent in the filter range?
            bool isAbsent = from != null && to != null && absences.Any(a =>
                a.UserId == u.UserId &&
                a.FromDate.Date <= to.Value.Date &&
                a.ToDate.Date   >= from.Value.Date);

            return new UserSummary
            {
                UserId    = u.UserId,
                UserName  = u.Name,
                Paid      = userPaid,
                FairShare = userOwes,
                Balance   = balance,
                IsAbsent  = isAbsent
            };
        }).ToList();

        // ── SETTLEMENTS (who pays whom) ──────────────────────────────────────
        var debtors   = userSummaries.Where(u => u.Balance < 0).OrderBy(u => u.Balance).ToList();
        var creditors = userSummaries.Where(u => u.Balance > 0).OrderByDescending(u => u.Balance).ToList();
        var settlements = new List<SettlementItem>();
        var cBal = creditors.Select(c => c.Balance).ToArray();
        var dBal = debtors.Select(d => Math.Abs(d.Balance)).ToArray();
        int ci = 0, di = 0;
        while (ci < creditors.Count && di < debtors.Count)
        {
            var amt = Math.Min(cBal[ci], dBal[di]);
            if (amt > 0.01m)
                settlements.Add(new SettlementItem
                {
                    From   = debtors[di].UserName,
                    To     = creditors[ci].UserName,
                    Amount = Math.Round(amt, 2)
                });
            cBal[ci] -= amt; dBal[di] -= amt;
            if (cBal[ci] < 0.01m) ci++;
            if (dBal[di] < 0.01m) di++;
        }

        // ── CATEGORY BREAKDOWN ───────────────────────────────────────────────
        var categoryBreakdown = allExpenses
            .GroupBy(e => e.Category)
            .Select(g => new CategorySummary
            {
                Category   = g.Key,
                Total      = g.Sum(e => e.Amount),
                Count      = g.Count(),
                Percentage = grandTotal > 0
                    ? Math.Round(g.Sum(e => e.Amount) / grandTotal * 100, 1)
                    : 0
            })
            .OrderByDescending(c => c.Total)
            .ToList();

        var me = userSummaries.FirstOrDefault(u => u.UserId == currentUserId);

        return new SummaryViewModel
        {
            GrandTotal        = grandTotal,
            TotalExpenses     = allExpenses.Count,
            MyTotal           = paid.ContainsKey(currentUserId) ? paid[currentUserId] : 0,
            FairShare         = me?.FairShare ?? 0,
            MyBalance         = me?.Balance   ?? 0,
            UserSummaries     = userSummaries,
            CategoryBreakdown = categoryBreakdown,
            Settlements       = settlements,
            FilterFrom        = from,
            FilterTo          = to
        };
    }
}
