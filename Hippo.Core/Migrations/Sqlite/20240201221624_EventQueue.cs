using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class EventQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueuedEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RequestId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueuedEvents_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueuedEvents_Action",
                table: "QueuedEvents",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedEvents_CreatedAt",
                table: "QueuedEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedEvents_RequestId",
                table: "QueuedEvents",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedEvents_Status",
                table: "QueuedEvents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedEvents_UpdatedAt",
                table: "QueuedEvents",
                column: "UpdatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueuedEvents");
        }
    }
}
