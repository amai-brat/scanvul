using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentResponseAndCreatedAtToCommand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "agent_response",
                table: "agent_command",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "agent_command",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "agent_response",
                table: "agent_command");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "agent_command");
        }
    }
}
