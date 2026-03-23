using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

#nullable disable
namespace ProductInventoryAPI.Migrations
{
    public partial class SeedUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Username", "Password", "Role" },
                values: new object[,]
                {
                    { "admin", "admin123", "Admin" },
                    { "manager", "manager123", "Manager" },
                    { "viewer", "viewer123", "Viewer" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Username",
                keyValues: new object[] { "admin", "manager", "viewer" });
        }
    }
}
