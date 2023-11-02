using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class RefactorPuppetSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Accounts_SponsorId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Accounts_AccountId",
                table: "Histories");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Groups_GroupId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Accounts_AccountId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Groups_GroupId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "GroupsAccounts");

            migrationBuilder.DropTable(
                name: "PuppetGroupsPuppetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Requests_AccountId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_GroupId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_GroupId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Histories_AccountId",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_SponsorId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SponsorId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "AccountStatus",
                table: "Histories",
                newName: "Status");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Requests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "Requests",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SshKey",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Requests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Accounts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Accounts",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kerberos",
                table: "Accounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GroupAdminAccount",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAdminAccount", x => new { x.AccountId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_GroupAdminAccount_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupAdminAccount_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMemberAccount",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMemberAccount", x => new { x.AccountId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_GroupMemberAccount_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMemberAccount_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TempGroups",
                columns: table => new
                {
                    Group = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempGroups", x => x.Group);
                });

            migrationBuilder.CreateTable(
                name: "TempKerberos",
                columns: table => new
                {
                    Kerberos = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempKerberos", x => x.Kerberos);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Group",
                table: "Requests",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Kerberos",
                table: "Accounts",
                column: "Kerberos");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAdminAccount_GroupId",
                table: "GroupAdminAccount",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberAccount_GroupId",
                table: "GroupMemberAccount",
                column: "GroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupAdminAccount");

            migrationBuilder.DropTable(
                name: "GroupMemberAccount");

            migrationBuilder.DropTable(
                name: "TempGroups");

            migrationBuilder.DropTable(
                name: "TempKerberos");

            migrationBuilder.DropIndex(
                name: "IX_Requests_Group",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Email",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Kerberos",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "SshKey",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Kerberos",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Histories",
                newName: "AccountStatus");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Permissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Histories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Groups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SponsorId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Accounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "GroupsAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "PuppetGroupsPuppetUsers",
                columns: table => new
                {
                    ClusterName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserKerberos = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuppetGroupsPuppetUsers", x => new { x.ClusterName, x.GroupName, x.UserKerberos });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_AccountId",
                table: "Requests",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_GroupId",
                table: "Requests",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_GroupId",
                table: "Permissions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_AccountId",
                table: "Histories",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SponsorId",
                table: "Accounts",
                column: "SponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupsAccounts_AccountId",
                table: "GroupsAccounts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupsAccounts_GroupId_AccountId",
                table: "GroupsAccounts",
                columns: new[] { "GroupId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PuppetGroupsPuppetUsers_GroupName",
                table: "PuppetGroupsPuppetUsers",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_PuppetGroupsPuppetUsers_UserKerberos",
                table: "PuppetGroupsPuppetUsers",
                column: "UserKerberos");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Accounts_SponsorId",
                table: "Accounts",
                column: "SponsorId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Accounts_AccountId",
                table: "Histories",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Groups_GroupId",
                table: "Permissions",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Accounts_AccountId",
                table: "Requests",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Groups_GroupId",
                table: "Requests",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
