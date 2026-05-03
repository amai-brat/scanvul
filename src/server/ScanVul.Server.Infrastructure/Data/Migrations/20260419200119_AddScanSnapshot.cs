using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScanSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "scan_snapshot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    computer_id = table.Column<long>(type: "bigint", nullable: false),
                    last_diff_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scan_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_scan_snapshot_computers_computer_id",
                        column: x => x.computer_id,
                        principalTable: "computers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scan_snapshot_diff",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    second_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scan_snapshot_diff", x => x.id);
                    table.ForeignKey(
                        name: "fk_scan_snapshot_diff_scan_snapshot_first_snapshot_id",
                        column: x => x.first_snapshot_id,
                        principalTable: "scan_snapshot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_scan_snapshot_diff_scan_snapshot_second_snapshot_id",
                        column: x => x.second_snapshot_id,
                        principalTable: "scan_snapshot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_scan_snapshot_computer_id",
                table: "scan_snapshot",
                column: "computer_id");

            migrationBuilder.CreateIndex(
                name: "ix_scan_snapshot_last_diff_id",
                table: "scan_snapshot",
                column: "last_diff_id");

            migrationBuilder.CreateIndex(
                name: "ix_scan_snapshot_diff_first_snapshot_id",
                table: "scan_snapshot_diff",
                column: "first_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "ix_scan_snapshot_diff_second_snapshot_id",
                table: "scan_snapshot_diff",
                column: "second_snapshot_id");

            migrationBuilder.AddForeignKey(
                name: "fk_scan_snapshot_scan_snapshot_diff_last_diff_id",
                table: "scan_snapshot",
                column: "last_diff_id",
                principalTable: "scan_snapshot_diff",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_scan_snapshot_scan_snapshot_diff_last_diff_id",
                table: "scan_snapshot");

            migrationBuilder.DropTable(
                name: "scan_snapshot_diff");

            migrationBuilder.DropTable(
                name: "scan_snapshot");
        }
    }
}
