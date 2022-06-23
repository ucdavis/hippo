using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class SshKeyPerCluster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SshKeyId",
                table: "Clusters",
                type: "TEXT",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SshName",
                table: "Clusters",
                type: "TEXT",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SshUrl",
                table: "Clusters",
                type: "TEXT",
                maxLength: 250,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SshKeyId",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "SshName",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "SshUrl",
                table: "Clusters");
        }
    }
}
