using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomExpenseTracker.Data;
using RoomExpenseTracker.Models;
using RoomExpenseTracker.Services;
using RoomExpenseTracker.ViewModels;
using System.Security.Claims;

namespace RoomExpenseTracker.Controllers;

[Authorize]
public class ExpensesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ExpenseService _svc;
    public ExpensesController(ApplicationDbContext db, ExpenseService svc) { _db = db; _svc = svc; }

    private string CurrentUserId   => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string CurrentUserName => User.FindFirstValue(ClaimTypes.Name)!;
    private bool   IsAdmin         => User.IsInRole("admin");

    // ── MY EXPENSES ──────────────────────────────────────────────────────
    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var expenses = await _svc.GetUserExpensesAsync(CurrentUserId, from, to);
        var vm = new ExpenseListViewModel
        {
            Expenses = expenses, TotalAmount = expenses.Sum(e => e.Amount),
            FilterFrom = from, FilterTo = to,
            CurrentUserId = CurrentUserId, CurrentUserName = CurrentUserName,
            IsAdmin = IsAdmin
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddExpenseViewModel vm)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Please fill all fields correctly."; return RedirectToAction("Index"); }
        _db.Expenses.Add(new Expense {
            UserId = CurrentUserId, UserName = CurrentUserName,
            Description = vm.Description, Amount = vm.Amount,
            Category = vm.Category, ExpenseDate = vm.ExpenseDate
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Expense added!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await _db.Expenses.FindAsync(id);
        if (e == null || (e.UserId != CurrentUserId && !IsAdmin)) return NotFound();
        return View(new EditExpenseViewModel { Id = e.Id, Description = e.Description, Amount = e.Amount, Category = e.Category, ExpenseDate = e.ExpenseDate });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditExpenseViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var e = await _db.Expenses.FindAsync(vm.Id);
        if (e == null || (e.UserId != CurrentUserId && !IsAdmin)) return NotFound();
        e.Description = vm.Description; e.Amount = vm.Amount;
        e.Category = vm.Category; e.ExpenseDate = vm.ExpenseDate;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Expense updated!";
        return IsAdmin ? RedirectToAction("Index", "Admin") : RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Expenses.FindAsync(id);
        if (e == null || (e.UserId != CurrentUserId && !IsAdmin)) return NotFound();
        _db.Expenses.Remove(e); await _db.SaveChangesAsync();
        TempData["Success"] = "Expense deleted!";
        return IsAdmin ? RedirectToAction("Index", "Admin") : RedirectToAction("Index");
    }

    // ── SUMMARY ──────────────────────────────────────────────────────────
    public async Task<IActionResult> Summary(DateTime? from, DateTime? to)
    {
        var vm = await _svc.GetSummaryAsync(CurrentUserId, from, to);
        return View(vm);
    }

    // ── BALANCES ─────────────────────────────────────────────────────────
    public async Task<IActionResult> Balances(DateTime? from, DateTime? to)
    {
        var vm = await _svc.GetSummaryAsync(CurrentUserId, from, to);
        return View(vm);
    }

    // ── ABSENCE ──────────────────────────────────────────────────────────
    public async Task<IActionResult> Absence()
    {
        ViewBag.Users = await _db.Users.Where(u => u.IsActive).ToListAsync();
        ViewBag.Absences = await _db.Absences.ToListAsync();
        return View(new AbsenceViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> MarkAbsent(AbsenceViewModel vm)
    {
        if (!ModelState.IsValid || vm.FromDate > vm.ToDate)
        { TempData["Error"] = "Invalid dates."; return RedirectToAction("Absence"); }
        // Remove existing absence for this user first
        var existing = await _db.Absences.Where(a => a.UserId == CurrentUserId).ToListAsync();
        _db.Absences.RemoveRange(existing);
        _db.Absences.Add(new Absence { UserId = CurrentUserId, FromDate = vm.FromDate, ToDate = vm.ToDate });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Absence marked!";
        return RedirectToAction("Absence");
    }

    [HttpPost]
    public async Task<IActionResult> ClearAbsence()
    {
        var existing = await _db.Absences.Where(a => a.UserId == CurrentUserId).ToListAsync();
        _db.Absences.RemoveRange(existing);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Marked as present!";
        return RedirectToAction("Absence");
    }
}
