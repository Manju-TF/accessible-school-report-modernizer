using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessibleSchoolReports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserSchoolAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSchoolAccess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    SchoolId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccessLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSchoolAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSchoolAccess_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSchoolAccess_SchoolId",
                table: "UserSchoolAccess",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSchoolAccess_UserId_SchoolId",
                table: "UserSchoolAccess",
                columns: new[] { "UserId", "SchoolId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSchoolAccess");
        }
    }
}
