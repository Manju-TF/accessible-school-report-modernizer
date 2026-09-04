using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessibleSchoolReports.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class KnowledgeDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnowledgeDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    DocumentType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ContentHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SourceIdentifier = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    IndexedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SchoolId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReportId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReportYear = table.Column<int>(type: "INTEGER", nullable: true),
                    ReportType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AuthorizationScope = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeDocuments_ReportRunItems_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportRunItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KnowledgeDocuments_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeChunks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KnowledgeDocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChunkNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    RuleId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SourceLocation = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Embedding = table.Column<byte[]>(type: "BLOB", nullable: true),
                    EmbeddingModel = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeChunks_KnowledgeDocuments_KnowledgeDocumentId",
                        column: x => x.KnowledgeDocumentId,
                        principalTable: "KnowledgeDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeChunks_KnowledgeDocumentId",
                table: "KnowledgeChunks",
                column: "KnowledgeDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeChunks_KnowledgeDocumentId_ChunkNumber",
                table: "KnowledgeChunks",
                columns: new[] { "KnowledgeDocumentId", "ChunkNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_AuthorizationScope",
                table: "KnowledgeDocuments",
                column: "AuthorizationScope");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_AuthorizationScope_SchoolId",
                table: "KnowledgeDocuments",
                columns: new[] { "AuthorizationScope", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_ContentHash",
                table: "KnowledgeDocuments",
                column: "ContentHash");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_ReportId",
                table: "KnowledgeDocuments",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_SchoolId",
                table: "KnowledgeDocuments",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_SourceIdentifier",
                table: "KnowledgeDocuments",
                column: "SourceIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnowledgeChunks");

            migrationBuilder.DropTable(
                name: "KnowledgeDocuments");
        }
    }
}
