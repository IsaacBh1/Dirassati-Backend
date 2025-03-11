using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class EditDataSeeders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 1,
                column: "Name",
                value: "Science");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 2,
                column: "Name",
                value: "Lettres");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 3,
                column: "Name",
                value: "Gestion et Économie");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 4,
                column: "Name",
                value: "Mathématiques");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 5,
                column: "Name",
                value: "Sciences Expérimentales");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 6,
                column: "Name",
                value: "Technique Mathématiques - Génie Civil");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 7,
                column: "Name",
                value: "Technique Mathématiques - Génie Électrique");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 8,
                column: "Name",
                value: "Technique Mathématiques - Génie Mécanique");

            migrationBuilder.InsertData(
                table: "Specializations",
                columns: new[] { "SpecializationId", "Name" },
                values: new object[,]
                {
                    { 9, "Lettres et Philosophie" },
                    { 10, "Langues Etrangeres" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 1,
                column: "Name",
                value: "Gestion et Économie");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 2,
                column: "Name",
                value: "Mathématiques");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 3,
                column: "Name",
                value: "Sciences Expérimentales");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 4,
                column: "Name",
                value: "Technique Mathématiques - Génie Civil");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 5,
                column: "Name",
                value: "Technique Mathématiques - Génie Électrique");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 6,
                column: "Name",
                value: "Technique Mathématiques - Génie Mécanique");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 7,
                column: "Name",
                value: "Primaire");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 8,
                column: "Name",
                value: "Moyenne");
        }
    }
}
