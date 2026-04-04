using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBduVulnerablePackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bdu_vulnerable_package",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bdu_id = table.Column<string>(type: "text", nullable: false),
                    package_info_id = table.Column<long>(type: "bigint", nullable: false),
                    computer_id = table.Column<long>(type: "bigint", nullable: false),
                    is_false_positive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bdu_vulnerable_package", x => x.id);
                    table.ForeignKey(
                        name: "fk_bdu_vulnerable_package_computers_computer_id",
                        column: x => x.computer_id,
                        principalTable: "computers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bdu_vulnerable_package_package_infos_package_info_id",
                        column: x => x.package_info_id,
                        principalTable: "package_infos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bdu_vulnerable_package_computer_id",
                table: "bdu_vulnerable_package",
                column: "computer_id");

            migrationBuilder.CreateIndex(
                name: "ix_bdu_vulnerable_package_package_info_id_bdu_id_computer_id",
                table: "bdu_vulnerable_package",
                columns: new[] { "package_info_id", "bdu_id", "computer_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bdu_vulnerable_package");
        }
    }
}
