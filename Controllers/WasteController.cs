using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomExpenseTracker.Data;
using RoomExpenseTracker.Models;
using RoomExpenseTracker.ViewModels;
using System.Security.Claims;

namespace RoomExpenseTracker.Controllers;

[Authorize]
public class WasteController : Controller
{
    private readonly ApplicationDbContext _db;
    public WasteController(ApplicationDbContext db) => _db = db;
    private bool IsAdmin => User.IsInRole("admin");

    public async Task<IActionResult> Index()
    {
        var today   = DateTime.Today;
        var todayRec = await _db.WasteThrows.FirstOrDefaultAsync(t => t.ThrowDate.Date == today);
        var latest   = await _db.WasteThrows.OrderByDescending(t => t.ThrowDate).FirstOrDefaultAsync();
        var history  = await _db.WasteThrows.OrderByDescending(t => t.ThrowDate).Take(10).ToListAsync();

        return View(new WasteTurnViewModel
        {
            Today       = todayRec,
            Latest      = latest,
            History     = history,
            IsAdmin     = IsAdmin,
            CurrentDate = today.ToString("dd MMMM yyyy, dddd")
        });
    }

    // User marks today as thrown
    [HttpPost]
    public async Task<IActionResult> MarkThrown(string throwerName)
    {
        if (string.IsNullOrWhiteSpace(throwerName))
        { TempData["Error"] = "Naam daalo!"; return RedirectToAction("Index"); }

        var today    = DateTime.Today;
        var existing = await _db.WasteThrows.FirstOrDefaultAsync(t => t.ThrowDate.Date == today);
        if (existing != null)
        {
            existing.ThrowerName = throwerName.Trim();
            existing.UpdatedAt   = DateTime.UtcNow;
        }
        else
        {
            _db.WasteThrows.Add(new WasteThrow
            {
                ThrowerName = throwerName.Trim(),
                ThrowDate   = today
            });
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = $"{throwerName} ne aaj kachra throw kiya!";
        return RedirectToAction("Index");
    }

    // Admin sets today's thrower + next person
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AdminSet(string throwerName, string? nextPersonName, string? date)
    {
        if (string.IsNullOrWhiteSpace(throwerName))
        { TempData["Error"] = "Naam daalo!"; return RedirectToAction("Index"); }

        var throwDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date).Date;
        var existing  = await _db.WasteThrows.FirstOrDefaultAsync(t => t.ThrowDate.Date == throwDate);

        if (existing != null)
        {
            existing.ThrowerName  = throwerName.Trim();
            existing.NextPersonName = nextPersonName?.Trim();
            existing.UpdatedAt    = DateTime.UtcNow;
        }
        else
        {
            _db.WasteThrows.Add(new WasteThrow
            {
                ThrowerName     = throwerName.Trim(),
                NextPersonName  = nextPersonName?.Trim(),
                ThrowDate       = throwDate
            });
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Update ho gaya!";
        return RedirectToAction("Index");
    }

    // Admin edits a past record
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> EditRecord(int id, string throwerName, string? nextPersonName)
    {
        var record = await _db.WasteThrows.FindAsync(id);
        if (record == null) return NotFound();
        record.ThrowerName   = throwerName.Trim();
        record.NextPersonName= nextPersonName?.Trim();
        record.UpdatedAt     = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Record update ho gaya!";
        return RedirectToAction("Index");
    }

    // Admin deletes a record
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteRecord(int id)
    {
        var record = await _db.WasteThrows.FindAsync(id);
        if (record != null) { _db.WasteThrows.Remove(record); await _db.SaveChangesAsync(); }
        TempData["Success"] = "Record delete ho gaya!";
        return RedirectToAction("Index");
    }

    // Admin resets all records
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ResetAll()
    {
        var all = await _db.WasteThrows.ToListAsync();
        _db.WasteThrows.RemoveRange(all);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Saara record reset ho gaya!";
        return RedirectToAction("Index");
    }
}
