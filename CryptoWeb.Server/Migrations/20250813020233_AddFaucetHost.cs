using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoWeb.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFaucetHost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaucetHosts",
                columns: table => new
                {
                    HostName = table.Column<string>(type: "TEXT", nullable: false),
                    BaseURL = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Delay = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxDayClaim = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxThread = table.Column<int>(type: "INTEGER", nullable: false),
                    Changed = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaucetHosts", x => x.HostName);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaucetHosts");
        }
    }
}
