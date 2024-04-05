﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class OpenOnDemand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Details",
                table: "Requests",
                newName: "Data");

            // copy data to json column (deprecated columns will be removed in future migration)
            migrationBuilder.Sql(@"
                UPDATE Requests SET Data = '{' || 
                    '""supervisingPI"":""' || IFNULL(SupervisingPI,'') || '"",' ||
                    '""sshKey"":""' || IFNULL(SshKey,'') || '"",' || 
                    '""openOnDemand"":false' || 
                '}'
                WHERE Action in ('CreateAccount','AddAccountToGroup')
            ");

            migrationBuilder.CreateTable(
                name: "AccessTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessTypeAccount",
                columns: table => new
                {
                    AccessTypesId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessTypeAccount", x => new { x.AccessTypesId, x.AccountsId });
                    table.ForeignKey(
                        name: "FK_AccessTypeAccount_AccessTypes_AccessTypesId",
                        column: x => x.AccessTypesId,
                        principalTable: "AccessTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessTypeAccount_Accounts_AccountsId",
                        column: x => x.AccountsId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessTypeCluster",
                columns: table => new
                {
                    AccessTypesId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClustersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessTypeCluster", x => new { x.AccessTypesId, x.ClustersId });
                    table.ForeignKey(
                        name: "FK_AccessTypeCluster_AccessTypes_AccessTypesId",
                        column: x => x.AccessTypesId,
                        principalTable: "AccessTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessTypeCluster_Clusters_ClustersId",
                        column: x => x.ClustersId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessTypeAccount_AccountsId",
                table: "AccessTypeAccount",
                column: "AccountsId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessTypeCluster_ClustersId",
                table: "AccessTypeCluster",
                column: "ClustersId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessTypes_Name",
                table: "AccessTypes",
                column: "Name",
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO AccessTypes (Id, Name) VALUES
                    (1, 'OpenOnDemand'), 
                    (2, 'SshKey');
            ");

            // create AccessTypeCluster records for existing clusters
            migrationBuilder.Sql(@"
                -- all clusters support AccessTypeId 1 (OpenOnDemand)
                INSERT INTO AccessTypeCluster (AccessTypesId, ClustersId)
                SELECT 1, Id FROM Clusters;
                INSERT INTO AccessTypeCluster (AccessTypesId, ClustersId)
                SELECT 2, Id FROM Clusters WHERE EnableUserSshKey = 1;
            ");

            // create AccessTypeAccount records for existing accounts
            migrationBuilder.Sql(@"
                -- all accounts support AccessTypeId 1 (OpenOnDemand)
                INSERT INTO AccessTypeAccount (AccessTypesId, AccountsId)
                SELECT 1, Id FROM Accounts;
                INSERT INTO AccessTypeAccount (AccessTypesId, AccountsId)
                SELECT 2, Id FROM Accounts WHERE IFNULL(SshKey, '') <> '';
            ");

            migrationBuilder.DropColumn(
                name: "EnableUserSshKey",
                table: "Clusters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // copy data from json back to individual columns
            migrationBuilder.Sql(@"UPDATE Requests SET
                SupervisingPI = JSON_EXTRACT(Data, '$.supervisingPI'),
                SshKey = JSON_EXTRACT(Data, '$.sshKey')
                WHERE Action in ('CreateAccount','AddAccountToGroup')
                AND IFNULL(Data, '') <> ''");

            migrationBuilder.RenameColumn(
                name: "Data",
                table: "Requests",
                newName: "Details");

            migrationBuilder.AddColumn<bool>(
                name: "EnableUserSshKey",
                table: "Clusters",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            // set EnableUserSshKey false if no AccessTypeCluster record is found
            migrationBuilder.Sql(@"
                UPDATE Clusters SET EnableUserSshKey=0
                WHERE Id NOT IN (SELECT ClustersId FROM AccessTypeCluster WHERE AccessTypesId=2)
            ");

            migrationBuilder.DropTable(
                name: "AccessTypeAccount");

            migrationBuilder.DropTable(
                name: "AccessTypeCluster");

            migrationBuilder.DropTable(
                name: "AccessTypes");

        }
    }
}
