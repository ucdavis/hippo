using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class AddClusterNameToSyncData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [PuppetGroupsPuppetUsers]");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PuppetGroupsPuppetUsers",
                table: "PuppetGroupsPuppetUsers");

            migrationBuilder.AddColumn<string>(
                name: "ClusterName",
                table: "PuppetGroupsPuppetUsers",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PuppetGroupsPuppetUsers",
                table: "PuppetGroupsPuppetUsers",
                columns: new[] { "ClusterName", "GroupName", "UserKerberos" });

            migrationBuilder.CreateIndex(
                name: "IX_PuppetGroupsPuppetUsers_GroupName",
                table: "PuppetGroupsPuppetUsers",
                column: "GroupName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [PuppetGroupsPuppetUsers]");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PuppetGroupsPuppetUsers",
                table: "PuppetGroupsPuppetUsers");

            migrationBuilder.DropIndex(
                name: "IX_PuppetGroupsPuppetUsers_GroupName",
                table: "PuppetGroupsPuppetUsers");

            migrationBuilder.DropColumn(
                name: "ClusterName",
                table: "PuppetGroupsPuppetUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PuppetGroupsPuppetUsers",
                table: "PuppetGroupsPuppetUsers",
                columns: new[] { "GroupName", "UserKerberos" });
        }
    }
}
