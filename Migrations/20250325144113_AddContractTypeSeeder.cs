using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddContractTypeSeeder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ContractTypes",
                columns: new[] { "ContractId", "Name" },
                values: new object[,]
                {
                    { 1, "Contrats Permanents" },
                    { 2, "Contrats à Durée Déterminée" },
                    { 3, "Contrats à Temps Partiel ou Horaire" },
                    { 4, "Stagiaire" },
                    { 5, "Consultant Pédagogique" },
                    { 6, "Bénévole" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContractTypes",
                keyColumn: "ContractId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ContractTypes",
                keyColumn: "ContractId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ContractTypes",
                keyColumn: "ContractId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ContractTypes",
                keyColumn: "ContractId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ContractTypes",
                keyColumn: "ContractId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ContractTypes",
                keyColumn: "ContractId",
                keyValue: 6);
        }
    }
}
