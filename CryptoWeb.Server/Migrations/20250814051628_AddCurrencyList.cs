using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoWeb.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "FaucetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UA",
                table: "FaucetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FaucetListCurrency",
                columns: table => new
                {
                    HostName = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaucetListCurrency", x => new { x.HostName, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "FaucetUserCurrency",
                columns: table => new
                {
                    HostName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<string>(type: "TEXT", nullable: false),
                    TotalClaim = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAmount = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaucetUserCurrency", x => new { x.HostName, x.Email, x.Name, x.Date });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaucetListCurrency");

            migrationBuilder.DropTable(
                name: "FaucetUserCurrency");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "FaucetUsers");

            migrationBuilder.DropColumn(
                name: "UA",
                table: "FaucetUsers");
        }
    }
}
