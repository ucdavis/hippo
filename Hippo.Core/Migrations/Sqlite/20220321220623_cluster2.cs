using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class cluster2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Clusters_ClusterId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Clusters_ClusterId1",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_ClusterId1",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ClusterId1",
                table: "Accounts");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Clusters_ClusterId",
                table: "Accounts",
                column: "ClusterId",
                principalTable: "Clusters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Clusters_ClusterId",
                table: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "ClusterId1",
                table: "Accounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ClusterId1",
                table: "Accounts",
                column: "ClusterId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Clusters_ClusterId",
                table: "Accounts",
                column: "ClusterId",
                principalTable: "Clusters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Clusters_ClusterId1",
                table: "Accounts",
                column: "ClusterId1",
                principalTable: "Clusters",
                principalColumn: "Id");
        }
    }
}
