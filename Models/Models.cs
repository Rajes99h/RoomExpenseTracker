using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomExpenseTracker.Models;

public class User
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string UserId { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(256)] public string PasswordHash { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string Role { get; set; } = "user";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

public class Expense
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string UserId { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string UserName { get; set; } = string.Empty;
    [Required, MaxLength(300)] public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(12,2)")] public decimal Amount { get; set; }
    [Required, MaxLength(50)] public string Category { get; set; } = "other";
    public DateTime ExpenseDate { get; set; } = DateTime.Today;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Absence
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string UserId { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SabjiTurn
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string UserId { get; set; } = string.Empty;
    public int DayOfWeek { get; set; } // 0=Sunday,1=Monday...6=Saturday
    public int WeekOrder { get; set; } // order in rotation
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SabjiPurchase
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string UserId { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string UserName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; } = DateTime.Today;
    public bool Purchased { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Simple Waste Throw Record — admin sets manually
public class WasteThrow
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string ThrowerName { get; set; } = string.Empty;
    public DateTime ThrowDate { get; set; } = DateTime.Today;
    [MaxLength(100)] public string? NextPersonName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class UserProfile
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string UserId { get; set; } = string.Empty;
    [MaxLength(10)] public string AvatarEmoji { get; set; } = "👤";
    [MaxLength(7)] public string AvatarColor { get; set; } = "#a855f7";
    [MaxLength(200)] public string? Bio { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
