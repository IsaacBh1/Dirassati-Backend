using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBillClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_SchoolTypes_SchoolTypeId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "BillAmount",
                table: "Schools");

            migrationBuilder.AddColumn<int>(
                name: "SchoolLevelId",
                table: "Bills",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bills_SchoolLevelId",
                table: "Bills",
                column: "SchoolLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_SchoolLevels_SchoolLevelId",
                table: "Bills",
                column: "SchoolLevelId",
                principalTable: "SchoolLevels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_SchoolTypes_SchoolTypeId",
                table: "Schools",
                column: "SchoolTypeId",
                principalTable: "SchoolTypes",
                principalColumn: "SchoolTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_SchoolLevels_SchoolLevelId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_SchoolTypes_SchoolTypeId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Bills_SchoolLevelId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "SchoolLevelId",
                table: "Bills");

            migrationBuilder.AddColumn<decimal>(
                name: "BillAmount",
                table: "Schools",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_SchoolTypes_SchoolTypeId",
                table: "Schools",
                column: "SchoolTypeId",
                principalTable: "SchoolTypes",
                principalColumn: "SchoolTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
