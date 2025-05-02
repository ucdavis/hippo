using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class ResourceAssociationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedOn",
                table: "GroupMemberAccount",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedOn",
                table: "Accounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupervisingPIId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberAccount_RevokedOn",
                table: "GroupMemberAccount",
                column: "RevokedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_DeactivatedOn",
                table: "Accounts",
                column: "DeactivatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SupervisingPIId",
                table: "Accounts",
                column: "SupervisingPIId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_SupervisingPIId",
                table: "Accounts",
                column: "SupervisingPIId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.Sql(
                "UPDATE Accounts SET DeactivatedOn = SYSUTCDATETIME() WHERE IsActive = 0");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Accounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(
                "UPDATE Accounts SET IsActive = 0 WHERE DeactivatedOn IS NOT NULL");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_SupervisingPIId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_GroupMemberAccount_RevokedOn",
                table: "GroupMemberAccount");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_DeactivatedOn",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_SupervisingPIId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RevokedOn",
                table: "GroupMemberAccount");

            migrationBuilder.DropColumn(
                name: "DeactivatedOn",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SupervisingPIId",
                table: "Accounts");
        }
    }
}
