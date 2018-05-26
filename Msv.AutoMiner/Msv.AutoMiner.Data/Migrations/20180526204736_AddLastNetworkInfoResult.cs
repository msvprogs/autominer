using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Msv.AutoMiner.Data.Migrations
{
    public partial class AddLastNetworkInfoResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastNetworkInfoMessage",
                table: "Coins",
                maxLength: 8192,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastNetworkInfoResult",
                table: "Coins",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastNetworkInfoMessage",
                table: "Coins");

            migrationBuilder.DropColumn(
                name: "LastNetworkInfoResult",
                table: "Coins");
        }
    }
}
