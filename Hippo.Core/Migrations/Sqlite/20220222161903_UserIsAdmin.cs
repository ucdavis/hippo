using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class UserIsAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountHistories_Accounts_AccountId",
                table: "AccountHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_OwnerId",
                table: "Accounts");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Iam",
                table: "Users",
                column: "Iam",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsAdmin",
                table: "Users",
                column: "IsAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CanSponsor",
                table: "Accounts",
                column: "CanSponsor");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CreatedOn",
                table: "Accounts",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UpdatedOn",
                table: "Accounts",
                column: "UpdatedOn");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountHistories_Accounts_AccountId",
                table: "AccountHistories",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_OwnerId",
                table: "Accounts",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountHistories_Accounts_AccountId",
                table: "AccountHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_OwnerId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Iam",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsAdmin",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CanSponsor",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CreatedOn",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Name",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_UpdatedOn",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountHistories_Accounts_AccountId",
                table: "AccountHistories",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_OwnerId",
                table: "Accounts",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
