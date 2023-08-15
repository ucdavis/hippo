using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class AddClusterNameToSyncData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE [PuppetGroupsPuppetUsers]");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PuppetGroupsPuppetUsers",
                table: "PuppetGroupsPuppetUsers");

            migrationBuilder.AddColumn<string>(
                name: "ClusterName",
                table: "PuppetGroupsPuppetUsers",
                type: "nvarchar(20)",
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
            migrationBuilder.Sql("TRUNCATE TABLE [PuppetGroupsPuppetUsers]");

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
