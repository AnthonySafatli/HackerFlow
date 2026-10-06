using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HackerFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddedConnectionsAndCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ReferralChance = table.Column<int>(type: "INTEGER", nullable: false),
                    Website = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Closeness = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    CanIntroduceMe = table.Column<bool>(type: "INTEGER", nullable: false),
                    IntroducedById = table.Column<int>(type: "INTEGER", nullable: true),
                    ConnectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastContactAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Connections_Connections_IntroducedById",
                        column: x => x.IntroducedById,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ConnectionCompanies",
                columns: table => new
                {
                    CompaniesId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectionsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionCompanies", x => new { x.CompaniesId, x.ConnectionsId });
                    table.ForeignKey(
                        name: "FK_ConnectionCompanies_Companies_CompaniesId",
                        column: x => x.CompaniesId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConnectionCompanies_Connections_ConnectionsId",
                        column: x => x.ConnectionsId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionCompanies_ConnectionsId",
                table: "ConnectionCompanies",
                column: "ConnectionsId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_IntroducedById",
                table: "Connections",
                column: "IntroducedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectionCompanies");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Connections");
        }
    }
}
