using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Msv.AutoMiner.Data.Migrations
{
    public partial class AddExchangeCurrencies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeCoins");

            migrationBuilder.CreateTable(
                name: "ExchangeCurrencies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CoinId = table.Column<Guid>(nullable: true),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Exchange = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    MinWithdrawAmount = table.Column<double>(nullable: false),
                    Symbol = table.Column<string>(maxLength: 16, nullable: false),
                    WithdrawalFee = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeCurrencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeCurrencies_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeCurrencies_CoinId",
                table: "ExchangeCurrencies",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeCurrencies_Symbol",
                table: "ExchangeCurrencies",
                column: "Symbol");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeCurrencies");

            migrationBuilder.CreateTable(
                name: "ExchangeCoins",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CoinId = table.Column<Guid>(nullable: true),
                    DateTime = table.Column<DateTime>(nullable: false),
                    Exchange = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    MinWithdrawAmount = table.Column<double>(nullable: false),
                    Symbol = table.Column<string>(maxLength: 16, nullable: false),
                    WithdrawalFee = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeCoins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeCoins_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeCoins_CoinId",
                table: "ExchangeCoins",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeCoins_Symbol",
                table: "ExchangeCoins",
                column: "Symbol");
        }
    }
}
