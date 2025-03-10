using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class addSeeders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ParentRelationshipToStudentTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Père" },
                    { 2, "Mère" },
                    { 3, "Tuteur légal" },
                    { 4, "Grand-parent" },
                    { 5, "Oncle/Tante" },
                    { 6, "Frère/Sœur majeur(e)" },
                    { 7, "Autre famille" }
                });

            migrationBuilder.InsertData(
                table: "SchoolTypes",
                columns: new[] { "SchoolTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Primaire" },
                    { 2, "Moyen" },
                    { 3, "Lycee" }
                });

            migrationBuilder.InsertData(
                table: "Specializations",
                columns: new[] { "SpecializationId", "Name" },
                values: new object[,]
                {
                    { 1, "Gestion et Économie" },
                    { 2, "Mathématiques" },
                    { 3, "Sciences Expérimentales" },
                    { 4, "Technique Mathématiques - Génie Civil" },
                    { 5, "Technique Mathématiques - Génie Électrique" },
                    { 6, "Technique Mathématiques - Génie Mécanique" },
                    { 7, "Primaire" },
                    { 8, "Moyenne" }
                });

            migrationBuilder.InsertData(
                table: "SchoolLevels",
                columns: new[] { "LevelId", "LevelYear", "SchoolTypeId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 1 },
                    { 3, 3, 1 },
                    { 4, 4, 1 },
                    { 5, 5, 1 },
                    { 6, 1, 2 },
                    { 7, 2, 2 },
                    { 8, 3, 2 },
                    { 9, 4, 2 },
                    { 10, 1, 3 },
                    { 11, 2, 3 },
                    { 12, 3, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ParentRelationshipToStudentTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ParentRelationshipToStudentTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ParentRelationshipToStudentTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ParentRelationshipToStudentTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ParentRelationshipToStudentTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ParentRelationshipToStudentTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ParentRelationshipToStudentTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "SchoolLevels",
                keyColumn: "LevelId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SchoolTypes",
                keyColumn: "SchoolTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SchoolTypes",
                keyColumn: "SchoolTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SchoolTypes",
                keyColumn: "SchoolTypeId",
                keyValue: 3);
        }
    }
}
