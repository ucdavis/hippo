using System;
using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class HippoV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // perform non-destructive operations first so that we can massage some data before we drop columns
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Histories",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Accounts",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Accounts",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kerberos",
                table: "Accounts",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    ClusterId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RequesterId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActorId = table.Column<int>(type: "INTEGER", nullable: true),
                    Group = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    ClusterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    SshKey = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SupervisingPI = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
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

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TempGroups",
                columns: table => new
                {
                    Group = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempGroups", x => x.Group);
                });

            migrationBuilder.CreateTable(
                name: "TempKerberos",
                columns: table => new
                {
                    Kerberos = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempKerberos", x => x.Kerberos);
                });

            migrationBuilder.CreateTable(
                name: "GroupAdminAccount",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClusterId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Permissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Permissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ClusterId_Name",
                table: "Groups",
                columns: new[] { "ClusterId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ClusterId",
                table: "Permissions",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RoleId",
                table: "Permissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UserId",
                table: "Permissions",
                column: "UserId");

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
                name: "IX_Requests_Group",
                table: "Requests",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequesterId",
                table: "Requests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status",
                table: "Requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);



            // Create some user permissions based on existing data before we drop those columns

            // Create some roles
            migrationBuilder.Sql($@"
                INSERT INTO Roles (Name) VALUES
                    ('{Role.Codes.System}'),
                    ('{Role.Codes.ClusterAdmin}')
                ");

            // User.IsAdmin -> Role.System
            migrationBuilder.Sql($@"
                INSERT INTO Permissions (RoleId, UserId)
                SELECT r.Id, u.Id
                FROM Users u JOIN Roles r ON r.Name = '{Role.Codes.System}'
                WHERE u.IsAdmin = 1
                ");

            // Account.IsAdmin -> Role.ClusterAdmin
            migrationBuilder.Sql($@"
                INSERT INTO Permissions (RoleId, UserId, ClusterId)
                SELECT DISTINCT r.Id, u.Id, a.ClusterId
                FROM Accounts a JOIN Users u ON u.Id = a.OwnerId
                    JOIN Roles r ON r.Name = '{Role.Codes.ClusterAdmin}'
                WHERE a.IsAdmin = 1
                ");

            // copy AccountHistories to Histories
            migrationBuilder.Sql(@"
                INSERT INTO Histories (ActedDate, ActedById, AdminAction, Action, Details, ClusterId, Status)
                SELECT ah.CreatedOn, ah.ActorId, 1, ah.Action, ah.Note, a.ClusterId, ah.Status
                FROM AccountHistories ah INNER JOIN Accounts a ON ah.AccountId = a.Id
            ");

            // extract ssh key from the yaml
            migrationBuilder.Sql(@"
                UPDATE [dbo].[Accounts]
                SET [SshKey] = TRIM(
                    SUBSTR(
                        SshKey,
                        INSTR(SshKey, 'ssh-'),
                        -- length is index of next newline minus index of 'ssh-' minus 1
                        INSTR(SUBSTR(SshKey, INSTR(SshKey, 'ssh-')), CHAR(10)) - 1
                    ),
                    '""'
                )
                WHERE [SshKey] LIKE '%ssh-%'
            ");


            // now it's safe to perform destructive operations
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Accounts_SponsorId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Accounts_AccountId",
                table: "Histories");

            migrationBuilder.DropTable(
                name: "AccountHistories");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsAdmin",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Histories_AccountId",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CanSponsor",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_IsAdmin",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_SponsorId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "CanSponsor",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SponsorId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Accounts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // perform non-destructive operations first so that we can massage some data before we drop columns
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Histories",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanSponsor",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SponsorId",
                table: "Accounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Accounts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

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
                name: "IX_Users_IsAdmin",
                table: "Users",
                column: "IsAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_AccountId",
                table: "Histories",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CanSponsor",
                table: "Accounts",
                column: "CanSponsor");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IsAdmin",
                table: "Accounts",
                column: "IsAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SponsorId",
                table: "Accounts",
                column: "SponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountHistories_AccountId",
                table: "AccountHistories",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountHistories_ActorId",
                table: "AccountHistories",
                column: "ActorId");

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



            // Restore what we can, like IsAdmin and CanSponsor based on permissions
            migrationBuilder.Sql($@"
                UPDATE Users
                SET Users.IsAdmin = 1
                FROM Users join Permissions on Permissions.UserId = Users.Id
                WHERE Permissions.RoleId = (SELECT Id FROM Roles WHERE Name = '{Role.Codes.System}')
                ");

            migrationBuilder.Sql($@"
                UPDATE Accounts
                SET Accounts.IsAdmin = 1
                FROM Accounts join Permissions
                    on Permissions.ClusterId = Accounts.ClusterId
                    and Permissions.UserId = Accounts.OwnerId
                WHERE Permissions.RoleId = (SELECT Id FROM Roles WHERE Name = '{Role.Codes.ClusterAdmin}')
                ");

            
            // copy relavant data from Histories to AccountHistories
            migrationBuilder.Sql(@"
                INSERT INTO AccountHistories (CreatedOn, ActorId, Action, Note, Status, AccountId)
                SELECT ActedDate, ActedById, Action, Details, AccountStatus, AccountId
                FROM Histories
                WHERE AccountStatus IS NOT NULL AND AccountId IS NOT NULL
            ");

            migrationBuilder.Sql(@"DELETE FROM Histories WHERE AccountStatus IS NOT NULL AND AccountId IS NOT NULL");


            // now it's safe to perform destructive operations
            migrationBuilder.DropTable(
                name: "GroupAdminAccount");

            migrationBuilder.DropTable(
                name: "GroupMemberAccount");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "TempGroups");

            migrationBuilder.DropTable(
                name: "TempKerberos");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Email",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Kerberos",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Kerberos",
                table: "Accounts");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
