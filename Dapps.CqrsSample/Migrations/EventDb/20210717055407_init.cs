using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dapps.CqrsSample.Migrations.EventDb
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aggregates",
                columns: table => new
                {
                    AggregateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Class = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Type = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aggregates", x => x.AggregateId);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    AggregateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Class = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => new { x.AggregateId, x.Version });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aggregates");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
