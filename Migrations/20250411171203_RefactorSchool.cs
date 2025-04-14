using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dirassati_Backend.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSchool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssueDate",
                table: "Bills");

            migrationBuilder.AddColumn<decimal>(
                name: "BillAmount",
                table: "Schools",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillAmount",
                table: "Schools");

            migrationBuilder.AddColumn<DateOnly>(
                name: "IssueDate",
                table: "Bills",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}
