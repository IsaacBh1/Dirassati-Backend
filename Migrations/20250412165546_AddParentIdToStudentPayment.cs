using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddParentIdToStudentPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "StudentPayments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StudentPayments_ParentId",
                table: "StudentPayments",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentPayments_Parents_ParentId",
                table: "StudentPayments",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "ParentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentPayments_Parents_ParentId",
                table: "StudentPayments");

            migrationBuilder.DropIndex(
                name: "IX_StudentPayments_ParentId",
                table: "StudentPayments");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "StudentPayments");
        }
    }
}
