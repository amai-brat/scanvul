using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVulnerablePackageStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_false_positive",
                table: "vulnerable_packages");

            migrationBuilder.DropColumn(
                name: "is_patchless",
                table: "vulnerable_packages");

            migrationBuilder.DropColumn(
                name: "is_false_positive",
                table: "bdu_vulnerable_packages");

            migrationBuilder.DropColumn(
                name: "is_patchless",
                table: "bdu_vulnerable_packages");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "vulnerable_packages",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "bdu_vulnerable_packages",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "vulnerable_packages");

            migrationBuilder.DropColumn(
                name: "status",
                table: "bdu_vulnerable_packages");

            migrationBuilder.AddColumn<bool>(
                name: "is_false_positive",
                table: "vulnerable_packages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_patchless",
                table: "vulnerable_packages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_false_positive",
                table: "bdu_vulnerable_packages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_patchless",
                table: "bdu_vulnerable_packages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
