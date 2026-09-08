using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessibleSchoolReports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImportRowSetHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RowSetSha256",
                table: "ImportRuns",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportRuns_RowSetSha256",
                table: "ImportRuns",
                column: "RowSetSha256");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportRuns_RowSetSha256",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "RowSetSha256",
                table: "ImportRuns");
        }
    }
}
