using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVulnerablePackageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vulnerable_package",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cve_id = table.Column<string>(type: "text", nullable: false),
                    package_info_id = table.Column<long>(type: "bigint", nullable: false),
                    computer_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vulnerable_package", x => x.id);
                    table.ForeignKey(
                        name: "fk_vulnerable_package_computers_computer_id",
                        column: x => x.computer_id,
                        principalTable: "computers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vulnerable_package_package_infos_package_info_id",
                        column: x => x.package_info_id,
                        principalTable: "package_infos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vulnerable_package_computer_id",
                table: "vulnerable_package",
                column: "computer_id");

            migrationBuilder.CreateIndex(
                name: "ix_vulnerable_package_package_info_id",
                table: "vulnerable_package",
                column: "package_info_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vulnerable_package");
        }
    }
}
