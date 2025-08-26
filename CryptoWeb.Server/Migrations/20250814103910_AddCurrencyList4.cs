using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoWeb.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyList4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSL",
                table: "FaucetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSL",
                table: "FaucetUsers");
        }
    }
}
