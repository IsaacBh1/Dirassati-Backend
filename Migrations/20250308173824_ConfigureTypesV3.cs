using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureTypesV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parents_RelationshipToStudents_RelationshipToStudentId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AcademicYears_AcademicYearId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Specializations_StreamId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "RelationshipToStudents");

            migrationBuilder.DropTable(
                name: "SchoolLevelSpecialization");

            migrationBuilder.DropIndex(
                name: "IX_Parents_RelationshipToStudentId",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "SchoolType",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "LevelType",
                table: "SchoolLevels");

            migrationBuilder.DropColumn(
                name: "RelationshipToStudentId",
                table: "Parents");

            migrationBuilder.RenameColumn(
                name: "StreamId",
                table: "Students",
                newName: "SchoolLevelId");

            migrationBuilder.RenameColumn(
                name: "AcademicYearId",
                table: "Students",
                newName: "ParentRelationshipToStudentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_StreamId",
                table: "Students",
                newName: "IX_Students_SchoolLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_AcademicYearId",
                table: "Students",
                newName: "IX_Students_ParentRelationshipToStudentTypeId");

            migrationBuilder.RenameColumn(
                name: "StreamId",
                table: "Specializations",
                newName: "SpecializationId");

            migrationBuilder.AddColumn<byte[]>(
                name: "PhotoUrl",
                table: "Students",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchooId",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "SpecializationId",
                table: "Students",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "Schools",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SchoolTypeId",
                table: "Schools",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SchoolTypeId",
                table: "SchoolLevels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdentityNumber",
                table: "Parents",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ParentRelationshipToStudentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentRelationshipToStudentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolSpecialization",
                columns: table => new
                {
                    SchoolsSchoolId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpecializationsSpecializationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolSpecialization", x => new { x.SchoolsSchoolId, x.SpecializationsSpecializationId });
                    table.ForeignKey(
                        name: "FK_SchoolSpecialization_Schools_SchoolsSchoolId",
                        column: x => x.SchoolsSchoolId,
                        principalTable: "Schools",
                        principalColumn: "SchoolId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchoolSpecialization_Specializations_SpecializationsSpecializationId",
                        column: x => x.SpecializationsSpecializationId,
                        principalTable: "Specializations",
                        principalColumn: "SpecializationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolTypes",
                columns: table => new
                {
                    SchoolTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolTypes", x => x.SchoolTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_ParentId",
                table: "Students",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolId",
                table: "Students",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SpecializationId",
                table: "Students",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_AcademicYearId",
                table: "Schools",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SchoolTypeId",
                table: "Schools",
                column: "SchoolTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolLevels_SchoolTypeId",
                table: "SchoolLevels",
                column: "SchoolTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolSpecialization_SpecializationsSpecializationId",
                table: "SchoolSpecialization",
                column: "SpecializationsSpecializationId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolLevels_SchoolTypes_SchoolTypeId",
                table: "SchoolLevels",
                column: "SchoolTypeId",
                principalTable: "SchoolTypes",
                principalColumn: "SchoolTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_AcademicYears_AcademicYearId",
                table: "Schools",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "AcademicYearId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_SchoolTypes_SchoolTypeId",
                table: "Schools",
                column: "SchoolTypeId",
                principalTable: "SchoolTypes",
                principalColumn: "SchoolTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_ParentRelationshipToStudentTypes_ParentRelationshipToStudentTypeId",
                table: "Students",
                column: "ParentRelationshipToStudentTypeId",
                principalTable: "ParentRelationshipToStudentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Parents_ParentId",
                table: "Students",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "ParentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_SchoolLevels_SchoolLevelId",
                table: "Students",
                column: "SchoolLevelId",
                principalTable: "SchoolLevels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "SchoolId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Specializations_SpecializationId",
                table: "Students",
                column: "SpecializationId",
                principalTable: "Specializations",
                principalColumn: "SpecializationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolLevels_SchoolTypes_SchoolTypeId",
                table: "SchoolLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_AcademicYears_AcademicYearId",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_SchoolTypes_SchoolTypeId",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_ParentRelationshipToStudentTypes_ParentRelationshipToStudentTypeId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Parents_ParentId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_SchoolLevels_SchoolLevelId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Specializations_SpecializationId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "ParentRelationshipToStudentTypes");

            migrationBuilder.DropTable(
                name: "SchoolSpecialization");

            migrationBuilder.DropTable(
                name: "SchoolTypes");

            migrationBuilder.DropIndex(
                name: "IX_Students_ParentId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_SchoolId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_SpecializationId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Schools_AcademicYearId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_SchoolTypeId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_SchoolLevels_SchoolTypeId",
                table: "SchoolLevels");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SchooId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SpecializationId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "SchoolTypeId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "SchoolTypeId",
                table: "SchoolLevels");

            migrationBuilder.DropColumn(
                name: "NationalIdentityNumber",
                table: "Parents");

            migrationBuilder.RenameColumn(
                name: "SchoolLevelId",
                table: "Students",
                newName: "StreamId");

            migrationBuilder.RenameColumn(
                name: "ParentRelationshipToStudentTypeId",
                table: "Students",
                newName: "AcademicYearId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_SchoolLevelId",
                table: "Students",
                newName: "IX_Students_StreamId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_ParentRelationshipToStudentTypeId",
                table: "Students",
                newName: "IX_Students_AcademicYearId");

            migrationBuilder.RenameColumn(
                name: "SpecializationId",
                table: "Specializations",
                newName: "StreamId");

            migrationBuilder.AddColumn<string>(
                name: "SchoolType",
                table: "Schools",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LevelType",
                table: "SchoolLevels",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RelationshipToStudentId",
                table: "Parents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RelationshipToStudents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationshipToStudents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolLevelSpecialization",
                columns: table => new
                {
                    SchoolLevelsLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    StreamsStreamId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolLevelSpecialization", x => new { x.SchoolLevelsLevelId, x.StreamsStreamId });
                    table.ForeignKey(
                        name: "FK_SchoolLevelSpecialization_SchoolLevels_SchoolLevelsLevelId",
                        column: x => x.SchoolLevelsLevelId,
                        principalTable: "SchoolLevels",
                        principalColumn: "LevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchoolLevelSpecialization_Specializations_StreamsStreamId",
                        column: x => x.StreamsStreamId,
                        principalTable: "Specializations",
                        principalColumn: "StreamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parents_RelationshipToStudentId",
                table: "Parents",
                column: "RelationshipToStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolLevelSpecialization_StreamsStreamId",
                table: "SchoolLevelSpecialization",
                column: "StreamsStreamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_RelationshipToStudents_RelationshipToStudentId",
                table: "Parents",
                column: "RelationshipToStudentId",
                principalTable: "RelationshipToStudents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AcademicYears_AcademicYearId",
                table: "Students",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "AcademicYearId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Specializations_StreamId",
                table: "Students",
                column: "StreamId",
                principalTable: "Specializations",
                principalColumn: "StreamId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
