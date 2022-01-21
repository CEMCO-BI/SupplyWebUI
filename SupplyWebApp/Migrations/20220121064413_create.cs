using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SupplyWebApp.Migrations
{
    public partial class create : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Upload");

            migrationBuilder.CreateTable(
                name: "CRUPricing",
                schema: "Upload",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(nullable: false),
                    Week1 = table.Column<double>(nullable: false),
                    Week2 = table.Column<double>(nullable: false),
                    Week3 = table.Column<double>(nullable: false),
                    Week4 = table.Column<double>(nullable: false),
                    Week5 = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CRUPricing", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlannedBuy",
                schema: "Upload",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Amount = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedBuy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesForecast",
                schema: "Upload",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Amount = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesForecast", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CRUPricing",
                schema: "Upload");

            migrationBuilder.DropTable(
                name: "PlannedBuy",
                schema: "Upload");

            migrationBuilder.DropTable(
                name: "SalesForecast",
                schema: "Upload");
        }
    }
}
