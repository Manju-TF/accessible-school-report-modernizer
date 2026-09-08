using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessibleSchoolReports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GraduateClassYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassYear",
                table: "GraduateRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GraduateRecords_ClassYear",
                table: "GraduateRecords",
                column: "ClassYear");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GraduateRecords_ClassYear",
                table: "GraduateRecords");

            migrationBuilder.DropColumn(
                name: "ClassYear",
                table: "GraduateRecords");
        }
    }
}
