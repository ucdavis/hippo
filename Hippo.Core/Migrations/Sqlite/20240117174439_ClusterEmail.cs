using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class ClusterEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clusters",
                type: "TEXT",
                maxLength: 250,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Clusters");
        }
    }
}
