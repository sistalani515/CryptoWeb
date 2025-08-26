using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoWeb.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddHoldFaucetHost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHold",
                table: "FaucetHosts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHold",
                table: "FaucetHosts");
        }
    }
}
