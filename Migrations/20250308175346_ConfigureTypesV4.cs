using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureTypesV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Schools_SchoolId",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_Schools_SchoolId1",
                table: "Classrooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_AspNetUsers_UserId1",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_AcademicYears_AcademicYearId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_AcademicYearId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Parents_UserId1",
                table: "Parents");

            migrationBuilder.DropIndex(
                name: "IX_Classrooms_SchoolId1",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "SchoolId1",
                table: "Classrooms");

            migrationBuilder.CreateIndex(
                name: "IX_Parents_UserId",
                table: "Parents",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_SchoolId",
                table: "Classrooms",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Schools_SchoolId",
                table: "AcademicYears",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_Schools_SchoolId",
                table: "Classrooms",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_AspNetUsers_UserId",
                table: "Parents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Schools_SchoolId",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_Schools_SchoolId",
                table: "Classrooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_AspNetUsers_UserId",
                table: "Parents");

            migrationBuilder.DropIndex(
                name: "IX_Parents_UserId",
                table: "Parents");

            migrationBuilder.DropIndex(
                name: "IX_Classrooms_SchoolId",
                table: "Classrooms");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Parents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId1",
                table: "Classrooms",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Schools_AcademicYearId",
                table: "Schools",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Parents_UserId1",
                table: "Parents",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_SchoolId1",
                table: "Classrooms",
                column: "SchoolId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Schools_SchoolId",
                table: "AcademicYears",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "SchoolId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_Schools_SchoolId1",
                table: "Classrooms",
                column: "SchoolId1",
                principalTable: "Schools",
                principalColumn: "SchoolId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_AspNetUsers_UserId1",
                table: "Parents",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_AcademicYears_AcademicYearId",
                table: "Schools",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "AcademicYearId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
