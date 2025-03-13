using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class MakeOneAcademicYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Schools_SchoolId",
                table: "AcademicYears");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_SchoolId",
                table: "AcademicYears");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_SchoolId",
                table: "AcademicYears",
                column: "SchoolId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Schools_SchoolId",
                table: "AcademicYears",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "SchoolId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Schools_SchoolId",
                table: "AcademicYears");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_SchoolId",
                table: "AcademicYears");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_SchoolId",
                table: "AcademicYears",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Schools_SchoolId",
                table: "AcademicYears",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "SchoolId");
        }
    }
}
