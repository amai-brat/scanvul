using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVulnerablePackageUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_vulnerable_package_package_info_id",
                table: "vulnerable_package");

            migrationBuilder.CreateIndex(
                name: "ix_vulnerable_package_package_info_id_cve_id_computer_id",
                table: "vulnerable_package",
                columns: new[] { "package_info_id", "cve_id", "computer_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_vulnerable_package_package_info_id_cve_id_computer_id",
                table: "vulnerable_package");

            migrationBuilder.CreateIndex(
                name: "ix_vulnerable_package_package_info_id",
                table: "vulnerable_package",
                column: "package_info_id");
        }
    }
}
