using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddReportEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTIme",
                table: "Absences",
                newName: "DateTime");

            migrationBuilder.CreateTable(
                name: "StudentReportStatuses",
                columns: table => new
                {
                    StudentReportStatusId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentReportStatuses", x => x.StudentReportStatusId);
                });

            migrationBuilder.CreateTable(
                name: "StudentReports",
                columns: table => new
                {
                    StudentReportId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeacherId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StudentReportStatusId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentReports", x => x.StudentReportId);
                    table.ForeignKey(
                        name: "FK_StudentReports_StudentReportStatuses_StudentReportStatusId",
                        column: x => x.StudentReportStatusId,
                        principalTable: "StudentReportStatuses",
                        principalColumn: "StudentReportStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentReports_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentReports_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "StudentReportStatuses",
                columns: new[] { "StudentReportStatusId", "Name" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Sent" },
                    { 3, "Viewed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_StudentId",
                table: "StudentReports",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_StudentReportStatusId",
                table: "StudentReports",
                column: "StudentReportStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_TeacherId",
                table: "StudentReports",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentReports");

            migrationBuilder.DropTable(
                name: "StudentReportStatuses");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "Absences",
                newName: "DateTIme");
        }
    }
}
