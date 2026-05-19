using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionsColumnToWingetPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "versions",
                table: "winget_packages",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "versions",
                table: "winget_packages");
        }
    }
}
