using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dapps.CqrsSample.Migrations.CommandDb
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AggregateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: true),
                    Class = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SendScheduled = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SendStarted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SendCompleted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SendCancelled = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SendStatus = table.Column<int>(type: "int", nullable: false),
                    SendError = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commands");
        }
    }
}
