using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drifters.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddTurnStatusAndPrompt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Turns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prompt",
                table: "Turns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Turns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SystemPrompt",
                table: "Turns",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "Prompt",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "SystemPrompt",
                table: "Turns");
        }
    }
}
