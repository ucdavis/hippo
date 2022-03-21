using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class adminHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActedById = table.Column<int>(type: "int", nullable: false),
                    AdminAction = table.Column<bool>(type: "bit", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Histories_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Histories_Users_ActedById",
                        column: x => x.ActedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Histories_AccountId",
                table: "Histories",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_ActedById",
                table: "Histories",
                column: "ActedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Histories");
        }
    }
}
