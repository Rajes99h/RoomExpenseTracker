using System.ComponentModel.DataAnnotations;

namespace RoomExpenseTracker.ViewModels;

public class LoginViewModel
{
    [Required] public string UserId { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
}

public class AddExpenseViewModel
{
    [Required, MaxLength(300)] public string Description { get; set; } = string.Empty;
    [Required, Range(0.01, 9999999)] public decimal Amount { get; set; }
    [Required] public string Category { get; set; } = "other";
    [Required] public DateTime ExpenseDate { get; set; } = DateTime.Today;
}

public class EditExpenseViewModel : AddExpenseViewModel
{
    public int Id { get; set; }
}

public class ExpenseListViewModel
{
    public List<RoomExpenseTracker.Models.Expense> Expenses { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime? FilterFrom { get; set; }
    public DateTime? FilterTo { get; set; }
    public string CurrentUserId { get; set; } = string.Empty;
    public string CurrentUserName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}

public class SummaryViewModel
{
    public decimal GrandTotal { get; set; }
    public int TotalExpenses { get; set; }
    public decimal MyTotal { get; set; }
    public decimal FairShare { get; set; }
    public decimal MyBalance { get; set; }
    public List<UserSummary> UserSummaries { get; set; } = new();
    public List<CategorySummary> CategoryBreakdown { get; set; } = new();
    public List<SettlementItem> Settlements { get; set; } = new();
    public DateTime? FilterFrom { get; set; }
    public DateTime? FilterTo { get; set; }
}

public class UserSummary
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal Paid { get; set; }
    public decimal FairShare { get; set; }
    public decimal Balance { get; set; }
    public bool IsAbsent { get; set; }
}

public class CategorySummary
{
    public string Category { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class SettlementItem
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class AbsenceViewModel
{
    [Required] public DateTime FromDate { get; set; } = DateTime.Today;
    [Required] public DateTime ToDate { get; set; } = DateTime.Today;
}

public class ChangePasswordViewModel
{
    [Required] public string OldPassword { get; set; } = string.Empty;
    [Required, MinLength(4)] public string NewPassword { get; set; } = string.Empty;
    [Required, Compare("NewPassword")] public string ConfirmPassword { get; set; } = string.Empty;
}

public class AdminUserViewModel
{
    public List<RoomExpenseTracker.Models.User> Users { get; set; } = new();
    public string? NewUserId { get; set; }
    public string? NewUserName { get; set; }
    public string? NewUserPassword { get; set; }
    public string NewUserRole { get; set; } = "user";
}

public class EditUserViewModel
{
    [Required] public string UserId { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
    [Required] public string Role { get; set; } = "user";
}

public class SabjiTurnViewModel
{
    public List<DayTurnInfo> WeekSchedule { get; set; } = new();
    public DayTurnInfo? Today { get; set; }
    public List<RoomExpenseTracker.Models.User> AllUsers { get; set; } = new();
    public bool IsAdmin { get; set; }
}

public class DayTurnInfo
{
    public DateTime Date { get; set; }
    public string DayName { get; set; } = string.Empty;
    public string? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public bool IsToday { get; set; }
    public bool IsPurchased { get; set; }
    public bool IsAbsent { get; set; }
    public string? ActualBuyerName { get; set; }
}

public class WasteTurnViewModel
{
    public RoomExpenseTracker.Models.WasteThrow? Today { get; set; }
    public RoomExpenseTracker.Models.WasteThrow? Latest { get; set; }
    public List<RoomExpenseTracker.Models.WasteThrow> History { get; set; } = new();
    public bool IsAdmin { get; set; }
    public string CurrentDate { get; set; } = string.Empty;
}

public class MonthlyReportViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public int TotalExpenses { get; set; }
    public decimal MyTotal { get; set; }
    public decimal FairShare { get; set; }
    public decimal MyBalance { get; set; }
    public List<UserSummary> UserSummaries { get; set; } = new();
    public List<CategorySummary> CategoryBreakdown { get; set; } = new();
    public List<SettlementItem> Settlements { get; set; } = new();
    public List<MonthOption> AvailableMonths { get; set; } = new();
    public Dictionary<string, string> UserAvatars { get; set; } = new();
    public Dictionary<string, string> UserColors { get; set; } = new();
    public DateTime? FilterFrom { get; set; }
    public DateTime? FilterTo { get; set; }
}

public class MonthOption
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class ProfileViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AvatarEmoji { get; set; } = "👤";
    public string AvatarColor { get; set; } = "#a855f7";
    public string? Bio { get; set; }
    public decimal TotalExpenses { get; set; }
    public int ExpenseCount { get; set; }
    public string Role { get; set; } = "user";
}
