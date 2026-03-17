using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomExpenseTracker.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new {
                    Id           = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId       = table.Column<string>(maxLength: 50, nullable: false),
                    Name         = table.Column<string>(maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(maxLength: 256, nullable: false),
                    Role         = table.Column<string>(maxLength: 20, nullable: false, defaultValue: "user"),
                    CreatedAt    = table.Column<DateTime>(nullable: false),
                    IsActive     = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table => { table.PrimaryKey("PK_Users", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new {
                    Id          = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId      = table.Column<string>(maxLength: 50, nullable: false),
                    UserName    = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 300, nullable: false),
                    Amount      = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Category    = table.Column<string>(maxLength: 50, nullable: false),
                    ExpenseDate = table.Column<DateTime>(nullable: false),
                    CreatedAt   = table.Column<DateTime>(nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Absences",
                columns: table => new {
                    Id        = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId    = table.Column<string>(maxLength: 50, nullable: false),
                    FromDate  = table.Column<DateTime>(nullable: false),
                    ToDate    = table.Column<DateTime>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Absences", x => x.Id);
                });

            migrationBuilder.CreateIndex("IX_Users_UserId",    "Users",    "UserId", unique: true);
            migrationBuilder.CreateIndex("IX_Expenses_UserId", "Expenses", "UserId");
            migrationBuilder.CreateIndex("IX_Expenses_Date",   "Expenses", "ExpenseDate");
            migrationBuilder.CreateIndex("IX_Absences_UserId", "Absences", "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Absences");
            migrationBuilder.DropTable("Expenses");
            migrationBuilder.DropTable("Users");
        }
    }
}
