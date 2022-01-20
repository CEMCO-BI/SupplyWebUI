using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SupplyWebApp.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CRUPricing",
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
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Amount = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedBuy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesForecast",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Amount = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesForecast", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CRUPricing");

            migrationBuilder.DropTable(
                name: "PlannedBuy");

            migrationBuilder.DropTable(
                name: "SalesForecast");
        }
    }
}
