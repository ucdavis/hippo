using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class MergeHistories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "Histories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // copy AccountHistories to Histories
            migrationBuilder.Sql(@"
                INSERT INTO Histories (ActedDate, ActedById, AdminAction, Action, Details, AccountId, ClusterId, AccountStatus)
                SELECT ah.CreatedOn, ah.ActorId, 1, ah.Action, ah.Note, ah.AccountId, a.ClusterId, ah.Status
                FROM AccountHistories ah INNER JOIN Accounts a ON ah.AccountId = a.Id
            ");

            migrationBuilder.DropTable(
                name: "AccountHistories");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    ActorId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountHistories_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountHistories_Users_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountHistories_AccountId",
                table: "AccountHistories",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountHistories_ActorId",
                table: "AccountHistories",
                column: "ActorId");

            // move relavant data from Histories to AccountHistories
            migrationBuilder.Sql(@"
                INSERT INTO AccountHistories (AccountId, ActorId, Action, CreatedOn, Note, Status)
                SELECT AccountId, ActedById, Action, ActedDate, Details, AccountStatus
                FROM Histories
                WHERE AccountStatus IS NOT NULL AND AccountId IS NOT NULL
            ");

            migrationBuilder.Sql(@"DELETE FROM Histories WHERE AccountStatus IS NOT NULL AND AccountId IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "Histories");

        }
    }
}
