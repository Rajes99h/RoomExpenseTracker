using Microsoft.EntityFrameworkCore;
using RoomExpenseTracker.Models;

namespace RoomExpenseTracker.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Absence> Absences => Set<Absence>();
    public DbSet<SabjiTurn> SabjiTurns => Set<SabjiTurn>();
    public DbSet<SabjiPurchase> SabjiPurchases => Set<SabjiPurchase>();
    public DbSet<WasteThrow> WasteThrows => Set<WasteThrow>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>().HasIndex(u => u.UserId).IsUnique();
        mb.Entity<Expense>().HasIndex(e => e.UserId);
        mb.Entity<Expense>().HasIndex(e => e.ExpenseDate);
        mb.Entity<Absence>().HasIndex(a => a.UserId);
        mb.Entity<SabjiTurn>().HasIndex(s => s.DayOfWeek);
        mb.Entity<SabjiPurchase>().HasIndex(s => s.PurchaseDate);
        mb.Entity<WasteThrow>().HasIndex(w => w.ThrowDate);
        mb.Entity<UserProfile>().HasIndex(p => p.UserId).IsUnique();
    }

    public void EnsureSeeded()
    {
        if (!Users.Any())
        {
            Users.AddRange(
                new User { UserId = "admin", Name = "Admin User",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "admin" },
                new User { UserId = "ravi",  Name = "Ravi Kumar",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"),  Role = "user" },
                new User { UserId = "priya", Name = "Priya Sharma",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"),  Role = "user" },
                new User { UserId = "amit",  Name = "Amit Singh",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"),  Role = "user" }
            );
            SaveChanges();
        }
    }
}
