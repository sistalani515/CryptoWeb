﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoWeb.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddHoldFaucetHost1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SLCompleted",
                table: "FaucetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SLCompleted",
                table: "FaucetUsers");
        }
    }
}
