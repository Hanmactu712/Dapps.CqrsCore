using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dapps.CqrsSample.Migrations.SnapshotDb
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Snapshots",
                columns: table => new
                {
                    AggregateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snapshots", x => x.AggregateId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Snapshots");
        }
    }
}
