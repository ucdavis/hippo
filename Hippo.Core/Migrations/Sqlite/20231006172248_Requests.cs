using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class Requests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RequesterId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActorId = table.Column<int>(type: "INTEGER", nullable: true),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    ClusterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Users_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_AccountId",
                table: "Requests",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Action",
                table: "Requests",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ActorId",
                table: "Requests",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ClusterId",
                table: "Requests",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_GroupId",
                table: "Requests",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequesterId",
                table: "Requests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status",
                table: "Requests",
                column: "Status");

            // create requests for existing account records
            migrationBuilder.Sql($@"
                INSERT INTO Requests (Action, RequesterId, ActorId, GroupId, AccountId, ClusterId, Status, Details)
                SELECT '{Request.ActionValues.CreateAccount}', OwnerId, NULL, ga.GroupId, a.Id, ClusterId, 
                    IIF(a.Status = 'Active', '{Request.StatusValues.Completed}', a.Status),
                    'Request created by migration for existing account. Actor details can be found in history table.'
                FROM Accounts a JOIN GroupsAccounts ga on ga.AccountId = a.Id
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requests");
        }
    }
}
