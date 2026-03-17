# 🏠 AAJ KI Tarif — Room Expense Tracker
## ASP.NET Core 8 MVC + MS SQL Server

---

## 🚀 Quick Setup

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full) — LocalDB comes with Visual Studio

### 2. Configure Connection String
Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RoomExpenseTracker;Trusted_Connection=True;"
}
```
For SQL Server Express: `Server=.\\SQLEXPRESS;Database=RoomExpenseTracker;Trusted_Connection=True;`
For full SQL Server: `Server=YOUR_SERVER;Database=RoomExpenseTracker;User Id=sa;Password=YOUR_PASSWORD;`

### 3. Run the App
```bash
dotnet restore
dotnet run
```
The app auto-creates the database and seeds default users on first run.

### 4. Default Login Credentials
| Username | Password  | Role  |
|----------|-----------|-------|
| admin    | admin123  | Admin |
| ravi     | pass123   | User  |
| priya    | pass123   | User  |
| amit     | pass123   | User  |

> ⚠️ Change passwords immediately after first login!

---

## 📁 Project Structure
```
RoomExpenseTracker/
├── Controllers/
│   ├── AccountController.cs     # Login, Logout, Change Password
│   ├── ExpensesController.cs    # My Expenses, Summary, Balances, Absence
│   └── AdminController.cs       # User management, All expenses
├── Models/
│   └── Models.cs                # User, Expense, Absence entities
├── ViewModels/
│   └── ViewModels.cs            # All view models
├── Data/
│   └── ApplicationDbContext.cs  # EF Core DbContext + seeding
├── Services/
│   └── ExpenseService.cs        # Business logic (summaries, balances)
├── Views/
│   ├── Account/                 # Login, ChangePassword
│   ├── Expenses/                # Index, Edit, Summary, Balances, Absence
│   ├── Admin/                   # Index, EditUser
│   └── Shared/                  # _Layout, _DashboardLayout
├── Migrations/                  # EF Core migrations
├── wwwroot/
│   ├── css/site.css             # Full stylesheet
│   └── js/site.js
├── Database/
│   └── CreateDatabase.sql       # Manual SQL script (optional)
├── appsettings.json
└── Program.cs
```

## 🛠️ Tech Stack
- **Framework**: ASP.NET Core 8 MVC
- **Database**: MS SQL Server via Entity Framework Core 8
- **Auth**: Cookie Authentication (BCrypt password hashing)
- **ORM**: EF Core with Code-First migrations

## ✨ Features
- 🔐 Login / Logout / Change Password
- 💸 Add, Edit, Delete personal expenses
- 🔍 Filter expenses by date range
- 📊 Summary with category breakdown & charts
- ⚖️ Balance calculator — who owes whom
- 📅 Mark absence (excluded from fair share)
- 👑 Admin panel: manage users, view/delete all expenses
