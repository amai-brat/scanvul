using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace ScanVul.Server.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchColumnToWingetPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                table: "winget_packages",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('english', coalesce(\"name\", '') || ' ' || coalesce(\"id\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_winget_packages_search_vector",
                table: "winget_packages",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_winget_packages_search_vector",
                table: "winget_packages");

            migrationBuilder.DropColumn(
                name: "search_vector",
                table: "winget_packages");
        }
    }
}
