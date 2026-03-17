using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomExpenseTracker.Data;
using RoomExpenseTracker.Models;
using RoomExpenseTracker.Services;
using RoomExpenseTracker.ViewModels;

namespace RoomExpenseTracker.Controllers;

[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ExpenseService _svc;
    public AdminController(ApplicationDbContext db, ExpenseService svc) { _db = db; _svc = svc; }

    public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? userId)
    {
        var expenses = await _svc.GetAllExpensesAsync(from, to, userId);
        var users    = await _db.Users.Where(u => u.IsActive).ToListAsync();
        ViewBag.Users      = users;
        ViewBag.Expenses   = expenses;
        ViewBag.Total      = expenses.Sum(e => e.Amount);
        ViewBag.Count      = expenses.Count;
        ViewBag.FilterFrom = from?.ToString("yyyy-MM-dd");
        ViewBag.FilterTo   = to?.ToString("yyyy-MM-dd");
        ViewBag.FilterUser = userId;
        return View(new AdminUserViewModel { Users = users });
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(AdminUserViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.NewUserId) ||
            string.IsNullOrWhiteSpace(vm.NewUserName) ||
            string.IsNullOrWhiteSpace(vm.NewUserPassword))
        { TempData["Error"] = "Please fill all fields."; return RedirectToAction("Index"); }

        if (await _db.Users.AnyAsync(u => u.UserId == vm.NewUserId))
        { TempData["Error"] = "User ID already exists."; return RedirectToAction("Index"); }

        _db.Users.Add(new User {
            UserId       = vm.NewUserId!,
            Name         = vm.NewUserName!,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.NewUserPassword!),
            Role         = vm.NewUserRole
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "User added!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.UserId == id);
        if (u == null) return NotFound();
        return View(new EditUserViewModel { UserId = u.UserId, Name = u.Name, Role = u.Role });
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var u = await _db.Users.FirstOrDefaultAsync(x => x.UserId == vm.UserId);
        if (u == null) return NotFound();
        u.Name = vm.Name;
        u.Role = vm.Role;
        if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
        // Update UserName in expenses
        var userExpenses = await _db.Expenses.Where(e => e.UserId == vm.UserId).ToListAsync();
        foreach (var e in userExpenses) e.UserName = vm.Name;
        await _db.SaveChangesAsync();
        TempData["Success"] = "User updated!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.UserId == id);
        if (u == null) return NotFound();
        var expenses = await _db.Expenses.Where(e => e.UserId == id).ToListAsync();
        var absences = await _db.Absences.Where(a => a.UserId == id).ToListAsync();
        _db.Expenses.RemoveRange(expenses);
        _db.Absences.RemoveRange(absences);
        _db.Users.Remove(u);
        await _db.SaveChangesAsync();
        TempData["Success"] = "User deleted.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAllExpenses()
    {
        var all = await _db.Expenses.ToListAsync();
        _db.Expenses.RemoveRange(all);
        await _db.SaveChangesAsync();
        TempData["Success"] = "All expenses deleted.";
        return RedirectToAction("Index");
    }
}
