using Hippo.Core.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class Groups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Accounts",
                type: "INTEGER",
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
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClusterId = table.Column<int>(type: "INTEGER", nullable: true),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: true)
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
                        name: "FK_Permissions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
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
                name: "IX_Accounts_GroupId",
                table: "Accounts",
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
                name: "IX_Permissions_GroupId",
                table: "Permissions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RoleId",
                table: "Permissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UserId",
                table: "Permissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Groups_GroupId",
                table: "Accounts",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);


            // Create some user permissions based on existing data before we drop those columns

            // Create some roles
            migrationBuilder.Sql($@"
                INSERT INTO Roles (Name) VALUES
                    ('{Role.Codes.System}'),
                    ('{Role.Codes.ClusterAdmin}'),
                    ('{Role.Codes.GroupAdmin}'),
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

            // Account.CanSponsor -> Role.GroupAdmin
            // Leaving GroupId null isn't valid for GroupAdmin role, but we don't have that information.
            // We'll have the PuppetSync job fix this up later.
            migrationBuilder.Sql($@"
                INSERT INTO Permissions (RoleId, UserId, ClusterId)
                SELECT DISTINCT r.Id, u.Id, a.ClusterId
                FROM Accounts a JOIN Users u ON u.Id = a.OwnerId
                    JOIN Roles r ON r.Name = '{Role.Codes.GroupAdmin}'
                WHERE a.CanSponsor = 1
                "); 



            migrationBuilder.DropIndex(
                name: "IX_Users_IsAdmin",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CanSponsor",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_IsAdmin",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CanSponsor",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Accounts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSponsor",
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

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsAdmin",
                table: "Users",
                column: "IsAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CanSponsor",
                table: "Accounts",
                column: "CanSponsor");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IsAdmin",
                table: "Accounts",
                column: "IsAdmin");


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

            migrationBuilder.Sql($@"
                UPDATE Accounts
                SET Accounts.CanSponsor = 1
                FROM Accounts join Permissions
                    on Permissions.ClusterId = Accounts.ClusterId
                    and Permissions.UserId = Accounts.OwnerId
                WHERE Permissions.RoleId = (SELECT Id FROM Roles WHERE Name = '{Role.Codes.GroupAdmin}')
            ");


            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Groups_GroupId",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_GroupId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Accounts");
        }
    }
}
