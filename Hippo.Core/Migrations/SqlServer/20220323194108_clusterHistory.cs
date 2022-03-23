using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class clusterHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClusterId",
                table: "Histories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Histories_ClusterId",
                table: "Histories",
                column: "ClusterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Clusters_ClusterId",
                table: "Histories",
                column: "ClusterId",
                principalTable: "Clusters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Clusters_ClusterId",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Histories_ClusterId",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "Histories");
        }
    }
}
