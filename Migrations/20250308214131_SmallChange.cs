using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class SmallChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SchoolTypes",
                keyColumn: "SchoolTypeId",
                keyValue: 2,
                column: "Name",
                value: "Moyenne");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SchoolTypes",
                keyColumn: "SchoolTypeId",
                keyValue: 2,
                column: "Name",
                value: "Moyen");
        }
    }
}
