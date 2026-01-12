using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFalsePositiveColumnToVulnerablePackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vulnerable_package_computers_computer_id",
                table: "vulnerable_package");

            migrationBuilder.DropForeignKey(
                name: "fk_vulnerable_package_package_infos_package_info_id",
                table: "vulnerable_package");

            migrationBuilder.DropPrimaryKey(
                name: "pk_vulnerable_package",
                table: "vulnerable_package");

            migrationBuilder.RenameTable(
                name: "vulnerable_package",
                newName: "vulnerable_packages");

            migrationBuilder.RenameIndex(
                name: "ix_vulnerable_package_package_info_id_cve_id_computer_id",
                table: "vulnerable_packages",
                newName: "ix_vulnerable_packages_package_info_id_cve_id_computer_id");

            migrationBuilder.RenameIndex(
                name: "ix_vulnerable_package_computer_id",
                table: "vulnerable_packages",
                newName: "ix_vulnerable_packages_computer_id");

            migrationBuilder.AddColumn<bool>(
                name: "is_false_positive",
                table: "vulnerable_packages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "pk_vulnerable_packages",
                table: "vulnerable_packages",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_vulnerable_packages_computers_computer_id",
                table: "vulnerable_packages",
                column: "computer_id",
                principalTable: "computers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vulnerable_packages_package_infos_package_info_id",
                table: "vulnerable_packages",
                column: "package_info_id",
                principalTable: "package_infos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vulnerable_packages_computers_computer_id",
                table: "vulnerable_packages");

            migrationBuilder.DropForeignKey(
                name: "fk_vulnerable_packages_package_infos_package_info_id",
                table: "vulnerable_packages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_vulnerable_packages",
                table: "vulnerable_packages");

            migrationBuilder.DropColumn(
                name: "is_false_positive",
                table: "vulnerable_packages");

            migrationBuilder.RenameTable(
                name: "vulnerable_packages",
                newName: "vulnerable_package");

            migrationBuilder.RenameIndex(
                name: "ix_vulnerable_packages_package_info_id_cve_id_computer_id",
                table: "vulnerable_package",
                newName: "ix_vulnerable_package_package_info_id_cve_id_computer_id");

            migrationBuilder.RenameIndex(
                name: "ix_vulnerable_packages_computer_id",
                table: "vulnerable_package",
                newName: "ix_vulnerable_package_computer_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_vulnerable_package",
                table: "vulnerable_package",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_vulnerable_package_computers_computer_id",
                table: "vulnerable_package",
                column: "computer_id",
                principalTable: "computers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vulnerable_package_package_infos_package_info_id",
                table: "vulnerable_package",
                column: "package_info_id",
                principalTable: "package_infos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
