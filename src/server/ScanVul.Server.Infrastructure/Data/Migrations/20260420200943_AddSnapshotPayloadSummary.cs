using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSnapshotPayloadSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_scan_snapshot_computers_computer_id",
                table: "scan_snapshot");

            migrationBuilder.DropForeignKey(
                name: "fk_scan_snapshot_scan_snapshot_diff_last_diff_id",
                table: "scan_snapshot");

            migrationBuilder.DropForeignKey(
                name: "fk_scan_snapshot_diff_scan_snapshot_first_snapshot_id",
                table: "scan_snapshot_diff");

            migrationBuilder.DropForeignKey(
                name: "fk_scan_snapshot_diff_scan_snapshot_second_snapshot_id",
                table: "scan_snapshot_diff");

            migrationBuilder.DropPrimaryKey(
                name: "pk_scan_snapshot",
                table: "scan_snapshot");

            migrationBuilder.RenameTable(
                name: "scan_snapshot",
                newName: "scan_snapshots");

            migrationBuilder.RenameIndex(
                name: "ix_scan_snapshot_last_diff_id",
                table: "scan_snapshots",
                newName: "ix_scan_snapshots_last_diff_id");

            migrationBuilder.RenameIndex(
                name: "ix_scan_snapshot_computer_id",
                table: "scan_snapshots",
                newName: "ix_scan_snapshots_computer_id");

            migrationBuilder.AddColumn<string>(
                name: "summary",
                table: "scan_snapshot_diff",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "summary",
                table: "scan_snapshots",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddPrimaryKey(
                name: "pk_scan_snapshots",
                table: "scan_snapshots",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_scan_snapshot_diff_scan_snapshots_first_snapshot_id",
                table: "scan_snapshot_diff",
                column: "first_snapshot_id",
                principalTable: "scan_snapshots",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_scan_snapshot_diff_scan_snapshots_second_snapshot_id",
                table: "scan_snapshot_diff",
                column: "second_snapshot_id",
                principalTable: "scan_snapshots",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_scan_snapshots_computers_computer_id",
                table: "scan_snapshots",
                column: "computer_id",
                principalTable: "computers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_scan_snapshots_scan_snapshot_diff_last_diff_id",
                table: "scan_snapshots",
                column: "last_diff_id",
                principalTable: "scan_snapshot_diff",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_scan_snapshot_diff_scan_snapshots_first_snapshot_id",
                table: "scan_snapshot_diff");

            migrationBuilder.DropForeignKey(
                name: "fk_scan_snapshot_diff_scan_snapshots_second_snapshot_id",
                table: "scan_snapshot_diff");

            migrationBuilder.DropForeignKey(
                name: "fk_scan_snapshots_computers_computer_id",
                table: "scan_snapshots");

            migrationBuilder.DropForeignKey(
                name: "fk_scan_snapshots_scan_snapshot_diff_last_diff_id",
                table: "scan_snapshots");

            migrationBuilder.DropPrimaryKey(
                name: "pk_scan_snapshots",
                table: "scan_snapshots");

            migrationBuilder.DropColumn(
                name: "summary",
                table: "scan_snapshot_diff");

            migrationBuilder.DropColumn(
                name: "summary",
                table: "scan_snapshots");

            migrationBuilder.RenameTable(
                name: "scan_snapshots",
                newName: "scan_snapshot");

            migrationBuilder.RenameIndex(
                name: "ix_scan_snapshots_last_diff_id",
                table: "scan_snapshot",
                newName: "ix_scan_snapshot_last_diff_id");

            migrationBuilder.RenameIndex(
                name: "ix_scan_snapshots_computer_id",
                table: "scan_snapshot",
                newName: "ix_scan_snapshot_computer_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_scan_snapshot",
                table: "scan_snapshot",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_scan_snapshot_computers_computer_id",
                table: "scan_snapshot",
                column: "computer_id",
                principalTable: "computers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_scan_snapshot_scan_snapshot_diff_last_diff_id",
                table: "scan_snapshot",
                column: "last_diff_id",
                principalTable: "scan_snapshot_diff",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_scan_snapshot_diff_scan_snapshot_first_snapshot_id",
                table: "scan_snapshot_diff",
                column: "first_snapshot_id",
                principalTable: "scan_snapshot",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_scan_snapshot_diff_scan_snapshot_second_snapshot_id",
                table: "scan_snapshot_diff",
                column: "second_snapshot_id",
                principalTable: "scan_snapshot",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
