using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatchlessColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "cve_id",
                table: "vulnerable_packages",
                newName: "vulnerability_id");

            migrationBuilder.RenameIndex(
                name: "ix_vulnerable_packages_package_info_id_cve_id_computer_id",
                table: "vulnerable_packages",
                newName: "ix_vulnerable_packages_package_info_id_vulnerability_id_comput");

            migrationBuilder.RenameColumn(
                name: "bdu_id",
                table: "bdu_vulnerable_packages",
                newName: "vulnerability_id");

            migrationBuilder.RenameIndex(
                name: "ix_bdu_vulnerable_packages_package_info_id_bdu_id_computer_id",
                table: "bdu_vulnerable_packages",
                newName: "ix_bdu_vulnerable_packages_package_info_id_vulnerability_id_co");

            migrationBuilder.AddColumn<bool>(
                name: "is_patchless",
                table: "vulnerable_packages",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_patchless",
                table: "vulnerable_packages");

            migrationBuilder.DropColumn(
                name: "is_patchless",
                table: "bdu_vulnerable_packages");

            migrationBuilder.RenameColumn(
                name: "vulnerability_id",
                table: "vulnerable_packages",
                newName: "cve_id");

            migrationBuilder.RenameIndex(
                name: "ix_vulnerable_packages_package_info_id_vulnerability_id_comput",
                table: "vulnerable_packages",
                newName: "ix_vulnerable_packages_package_info_id_cve_id_computer_id");

            migrationBuilder.RenameColumn(
                name: "vulnerability_id",
                table: "bdu_vulnerable_packages",
                newName: "bdu_id");

            migrationBuilder.RenameIndex(
                name: "ix_bdu_vulnerable_packages_package_info_id_vulnerability_id_co",
                table: "bdu_vulnerable_packages",
                newName: "ix_bdu_vulnerable_packages_package_info_id_bdu_id_computer_id");
        }
    }
}
