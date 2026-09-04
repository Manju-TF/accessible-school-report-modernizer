using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessibleSchoolReports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class KnowledgeGeneratedPdfMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportRunId",
                table: "KnowledgeDocuments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolCode",
                table: "KnowledgeDocuments",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_ReportRunId",
                table: "KnowledgeDocuments",
                column: "ReportRunId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_SchoolCode",
                table: "KnowledgeDocuments",
                column: "SchoolCode");

            migrationBuilder.AddForeignKey(
                name: "FK_KnowledgeDocuments_ReportRuns_ReportRunId",
                table: "KnowledgeDocuments",
                column: "ReportRunId",
                principalTable: "ReportRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KnowledgeDocuments_ReportRuns_ReportRunId",
                table: "KnowledgeDocuments");

            migrationBuilder.DropIndex(
                name: "IX_KnowledgeDocuments_ReportRunId",
                table: "KnowledgeDocuments");

            migrationBuilder.DropIndex(
                name: "IX_KnowledgeDocuments_SchoolCode",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "ReportRunId",
                table: "KnowledgeDocuments");

            migrationBuilder.DropColumn(
                name: "SchoolCode",
                table: "KnowledgeDocuments");
        }
    }
}
