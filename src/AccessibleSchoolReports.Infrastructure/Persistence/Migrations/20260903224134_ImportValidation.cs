using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessibleSchoolReports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImportValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlankRowCount",
                table: "ImportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContentSha256",
                table: "ImportRuns",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvalidRowCount",
                table: "ImportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ImportRowIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImportRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    RowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportRowIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportRowIssues_ImportRuns_ImportRunId",
                        column: x => x.ImportRunId,
                        principalTable: "ImportRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportRuns_ContentSha256",
                table: "ImportRuns",
                column: "ContentSha256");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRowIssues_ImportRunId",
                table: "ImportRowIssues",
                column: "ImportRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRowIssues_ImportRunId_RowNumber",
                table: "ImportRowIssues",
                columns: new[] { "ImportRunId", "RowNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportRowIssues");

            migrationBuilder.DropIndex(
                name: "IX_ImportRuns_ContentSha256",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "BlankRowCount",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "ContentSha256",
                table: "ImportRuns");

            migrationBuilder.DropColumn(
                name: "InvalidRowCount",
                table: "ImportRuns");
        }
    }
}
