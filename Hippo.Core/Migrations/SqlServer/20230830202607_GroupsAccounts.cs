using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class GroupsAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupsAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupsAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupsAccounts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupsAccounts_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupsAccounts_AccountId",
                table: "GroupsAccounts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupsAccounts_GroupId_AccountId",
                table: "GroupsAccounts",
                columns: new[] { "GroupId", "AccountId" },
                unique: true);

            // move data from Accounts.GroupId to GroupsAccounts
            migrationBuilder.Sql(@"
                INSERT INTO GroupsAccounts (GroupId, AccountId)
                SELECT GroupId, Id FROM Accounts
                WHERE GroupId IS NOT NULL");


            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Groups_GroupId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_GroupId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Accounts");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_GroupId",
                table: "Accounts",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Groups_GroupId",
                table: "Accounts",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // move data from GroupsAccounts to Accounts.GroupId (data loss if more than one group per account)
            migrationBuilder.Sql(@"
                UPDATE Accounts
                SET GroupId = ga.GroupId
                FROM GroupsAccounts ga
                WHERE Accounts.Id = ga.AccountId");

            migrationBuilder.DropTable(
                name: "GroupsAccounts");
        }
    }
}
