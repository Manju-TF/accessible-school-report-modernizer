using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessibleSchoolReports.Infrastructure.Persistence.Migrations;

[DbContext(typeof(SchoolReportsDbContext))]
[Migration("20261007150500_KnowledgeChunkFormatVersion")]
public sealed class KnowledgeChunkFormatVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ChunkFormatVersion",
            table: "KnowledgeDocuments",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ChunkFormatVersion",
            table: "KnowledgeDocuments");
    }
}
