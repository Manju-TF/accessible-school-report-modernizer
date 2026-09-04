using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessibleSchoolReports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReportRunTotals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DurationMilliseconds",
                table: "ReportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "FailedCount",
                table: "ReportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SuccessfulCount",
                table: "ReportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCount",
                table: "ReportRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMilliseconds",
                table: "ReportRuns");

            migrationBuilder.DropColumn(
                name: "FailedCount",
                table: "ReportRuns");

            migrationBuilder.DropColumn(
                name: "SuccessfulCount",
                table: "ReportRuns");

            migrationBuilder.DropColumn(
                name: "TotalCount",
                table: "ReportRuns");
        }
    }
}
