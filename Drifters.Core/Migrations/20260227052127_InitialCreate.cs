using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drifters.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Runs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InitialScenario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LmStudioBaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SetDesignerModel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaxTicks = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToolEventLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnId = table.Column<int>(type: "int", nullable: true),
                    ServerLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ToolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArgumentsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResultJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolEventLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RunId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SystemPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Objectives = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Motives = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_Runs_RunId",
                        column: x => x.RunId,
                        principalTable: "Runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ticks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RunId = table.Column<int>(type: "int", nullable: false),
                    TickNumber = table.Column<int>(type: "int", nullable: false),
                    SceneDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContinuationNarrative = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ticks_Runs_RunId",
                        column: x => x.RunId,
                        principalTable: "Runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TickId = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    CharacterReasoning = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToolCallName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ToolCallArguments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToolCallResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turns_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turns_Ticks_TickId",
                        column: x => x.TickId,
                        principalTable: "Ticks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorldStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TickId = table.Column<int>(type: "int", nullable: false),
                    StateJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecisionSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorldStates_Ticks_TickId",
                        column: x => x.TickId,
                        principalTable: "Ticks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_RunId",
                table: "Characters",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticks_RunId_TickNumber",
                table: "Ticks",
                columns: new[] { "RunId", "TickNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolEventLogs_CreatedAt",
                table: "ToolEventLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ToolEventLogs_TurnId",
                table: "ToolEventLogs",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_CharacterId",
                table: "Turns",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_TickId",
                table: "Turns",
                column: "TickId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldStates_TickId",
                table: "WorldStates",
                column: "TickId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToolEventLogs");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "WorldStates");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Ticks");

            migrationBuilder.DropTable(
                name: "Runs");
        }
    }
}
