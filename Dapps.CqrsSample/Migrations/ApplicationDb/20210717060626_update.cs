using Microsoft.EntityFrameworkCore.Migrations;

namespace Dapps.CqrsSample.Migrations.ApplicationDb
{
    public partial class update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Articles",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Articles",
                newName: "Summary");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Articles",
                newName: "Details");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Articles",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "Summary",
                table: "Articles",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Details",
                table: "Articles",
                newName: "FirstName");
        }
    }
}
