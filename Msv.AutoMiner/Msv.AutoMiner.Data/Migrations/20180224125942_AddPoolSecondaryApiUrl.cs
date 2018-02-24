using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Msv.AutoMiner.Data.Migrations
{
    public partial class AddPoolSecondaryApiUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiSecondaryUrl",
                table: "Pools",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiSecondaryUrl",
                table: "Pools");
        }
    }
}
