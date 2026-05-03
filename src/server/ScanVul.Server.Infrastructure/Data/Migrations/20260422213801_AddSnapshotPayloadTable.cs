using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSnapshotPayloadTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payload",
                table: "scan_snapshots");

            migrationBuilder.CreateTable(
                name: "scan_snapshot_payload",
                columns: table => new
                {
                    scan_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bdu_vulnerable_packages = table.Column<string>(type: "jsonb", nullable: false),
                    packages = table.Column<string>(type: "jsonb", nullable: false),
                    vulnerable_packages = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scan_snapshot_payload", x => x.scan_snapshot_id);
                    table.ForeignKey(
                        name: "fk_scan_snapshot_payload_scan_snapshots_scan_snapshot_id",
                        column: x => x.scan_snapshot_id,
                        principalTable: "scan_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scan_snapshot_payload");

            migrationBuilder.AddColumn<string>(
                name: "payload",
                table: "scan_snapshots",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }
    }
}
