using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class MergeHistories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "Histories",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            // copy AccountHistories to Histories
            migrationBuilder.Sql(@"
                INSERT INTO Histories (ActedDate, ActedById, AdminAction, Action, Details, AccountId, ClusterId, AccountStatus)
                SELECT ah.CreatedOn, ah.ActorId, 1, ah.Action, ah.Note, ah.AccountId, a.ClusterId, ah.Status
                FROM AccountHistories ah INNER JOIN Accounts a ON ah.AccountId = a.Id
            ");

            migrationBuilder.Sql(@"DELETE FROM Histories WHERE AccountStatus IS NOT NULL AND AccountId IS NOT NULL");

            migrationBuilder.DropTable(
                name: "AccountHistories");            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActorId = table.Column<int>(type: "INTEGER", nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1500, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
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

            // copy relavant data from Histories to AccountHistories
            migrationBuilder.Sql(@"
                INSERT INTO AccountHistories (CreatedOn, ActorId, Action, Note, Status, AccountId)
                SELECT ActedDate, ActedById, Action, Details, AccountStatus, AccountId
                FROM Histories
                WHERE AccountStatus IS NOT NULL AND AccountId IS NOT NULL
            ");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "Histories");

        }
    }
}
