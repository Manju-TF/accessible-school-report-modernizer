using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessibleSchoolReports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    StartedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CompletedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ImportedRowCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    StartedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CompletedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    MaxParallelism = table.Column<int>(type: "INTEGER", nullable: false),
                    OutputDirectory = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    Message = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GraduateRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImportRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    SchoolId = table.Column<int>(type: "INTEGER", nullable: false),
                    Sex3 = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Minstat = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Jobcat1 = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    JobFtPt = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Empgen = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Firm1 = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Lfjob = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Jobreg = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    LocationFlag = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Jobst = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Time1 = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Duration = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    SchoolFund = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    SalFtPerm = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: true),
                    Emptype1 = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GraduateRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GraduateRecords_ImportRuns_ImportRunId",
                        column: x => x.ImportRunId,
                        principalTable: "ImportRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GraduateRecords_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportRunItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReportRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    SchoolId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    OutputPath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    Message = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    StartedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CompletedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRunItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportRunItems_ReportRuns_ReportRunId",
                        column: x => x.ReportRunId,
                        principalTable: "ReportRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportRunItems_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GraduateRecords_ImportRunId",
                table: "GraduateRecords",
                column: "ImportRunId");

            migrationBuilder.CreateIndex(
                name: "IX_GraduateRecords_ImportRunId_SchoolId",
                table: "GraduateRecords",
                columns: new[] { "ImportRunId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_GraduateRecords_SchoolId",
                table: "GraduateRecords",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRuns_StartedUtc",
                table: "ImportRuns",
                column: "StartedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRunItems_ReportRunId",
                table: "ReportRunItems",
                column: "ReportRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRunItems_SchoolId",
                table: "ReportRunItems",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_StartedUtc",
                table: "ReportRuns",
                column: "StartedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_Code",
                table: "Schools",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GraduateRecords");

            migrationBuilder.DropTable(
                name: "ReportRunItems");

            migrationBuilder.DropTable(
                name: "ImportRuns");

            migrationBuilder.DropTable(
                name: "ReportRuns");

            migrationBuilder.DropTable(
                name: "Schools");
        }
    }
}
