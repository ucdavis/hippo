using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class PuppetSyncHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ActedById",
                table: "Histories",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_ActedDate",
                table: "Histories",
                column: "ActedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_Action",
                table: "Histories",
                column: "Action");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Histories_ActedDate",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Histories_Action",
                table: "Histories");

            migrationBuilder.AlterColumn<int>(
                name: "ActedById",
                table: "Histories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
