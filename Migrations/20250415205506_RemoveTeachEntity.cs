using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTeachEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Specializations_StreamId",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "Teaches");

            migrationBuilder.RenameColumn(
                name: "StreamId",
                table: "Groups",
                newName: "SpecializationId");

            migrationBuilder.RenameColumn(
                name: "GorupName",
                table: "Groups",
                newName: "GroupName");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_StreamId",
                table: "Groups",
                newName: "IX_Groups_SpecializationId");

            migrationBuilder.AlterColumn<Guid>(
                name: "GroupId",
                table: "Students",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "GroupId",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Specializations_SpecializationId",
                table: "Groups",
                column: "SpecializationId",
                principalTable: "Specializations",
                principalColumn: "SpecializationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Specializations_SpecializationId",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "SpecializationId",
                table: "Groups",
                newName: "StreamId");

            migrationBuilder.RenameColumn(
                name: "GroupName",
                table: "Groups",
                newName: "GorupName");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_SpecializationId",
                table: "Groups",
                newName: "IX_Groups_StreamId");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Students",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateTable(
                name: "Teaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    TeacherId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teaches_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Teaches_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Teaches_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teaches_GroupId",
                table: "Teaches",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Teaches_SubjectId",
                table: "Teaches",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Teaches_TeacherId",
                table: "Teaches",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Specializations_StreamId",
                table: "Groups",
                column: "StreamId",
                principalTable: "Specializations",
                principalColumn: "SpecializationId");
        }
    }
}
