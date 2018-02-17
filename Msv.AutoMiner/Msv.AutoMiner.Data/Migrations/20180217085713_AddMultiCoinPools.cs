using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Msv.AutoMiner.Data.Migrations
{
    public partial class AddMultiCoinPools : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MultiCoinPools",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Activity = table.Column<byte>(nullable: false),
                    ApiProtocol = table.Column<int>(nullable: false),
                    ApiUrl = table.Column<string>(maxLength: 256, nullable: true),
                    MiningUrl = table.Column<string>(maxLength: 256, nullable: true),
                    Name = table.Column<string>(maxLength: 64, nullable: false),
                    SiteUrl = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiCoinPools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MultiCoinPoolCurrencies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Algorithm = table.Column<string>(maxLength: 64, nullable: true),
                    Hashrate = table.Column<double>(nullable: false),
                    IsIgnored = table.Column<bool>(nullable: false),
                    MultiCoinPoolId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Port = table.Column<int>(nullable: false),
                    Symbol = table.Column<string>(maxLength: 64, nullable: false),
                    Workers = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiCoinPoolCurrencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiCoinPoolCurrencies_MultiCoinPools_MultiCoinPoolId",
                        column: x => x.MultiCoinPoolId,
                        principalTable: "MultiCoinPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MultiCoinPoolCurrencies_MultiCoinPoolId",
                table: "MultiCoinPoolCurrencies",
                column: "MultiCoinPoolId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MultiCoinPoolCurrencies");

            migrationBuilder.DropTable(
                name: "MultiCoinPools");
        }
    }
}
