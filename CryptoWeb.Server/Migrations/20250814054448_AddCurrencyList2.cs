using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoWeb.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyList2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FaucetUserCurrency",
                table: "FaucetUserCurrency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FaucetListCurrency",
                table: "FaucetListCurrency");

            migrationBuilder.RenameTable(
                name: "FaucetUserCurrency",
                newName: "FaucetUserCurrencies");

            migrationBuilder.RenameTable(
                name: "FaucetListCurrency",
                newName: "FaucetListCurrencies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FaucetUserCurrencies",
                table: "FaucetUserCurrencies",
                columns: new[] { "HostName", "Email", "Name", "Date" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_FaucetListCurrencies",
                table: "FaucetListCurrencies",
                columns: new[] { "HostName", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FaucetUserCurrencies",
                table: "FaucetUserCurrencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FaucetListCurrencies",
                table: "FaucetListCurrencies");

            migrationBuilder.RenameTable(
                name: "FaucetUserCurrencies",
                newName: "FaucetUserCurrency");

            migrationBuilder.RenameTable(
                name: "FaucetListCurrencies",
                newName: "FaucetListCurrency");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FaucetUserCurrency",
                table: "FaucetUserCurrency",
                columns: new[] { "HostName", "Email", "Name", "Date" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_FaucetListCurrency",
                table: "FaucetListCurrency",
                columns: new[] { "HostName", "Name" });
        }
    }
}
