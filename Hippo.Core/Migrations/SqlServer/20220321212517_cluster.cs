using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class cluster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClusterId",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClusterId1",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ClusterId",
                table: "Accounts",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ClusterId1",
                table: "Accounts",
                column: "ClusterId1");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IsAdmin",
                table: "Accounts",
                column: "IsAdmin");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Clusters_ClusterId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Clusters_ClusterId1",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_ClusterId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_ClusterId1",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_IsAdmin",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ClusterId1",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Accounts");
        }
    }
}
