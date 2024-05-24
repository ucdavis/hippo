using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class ReplaceFlagWithType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowToUser",
                table: "Histories");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Histories",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Histories_Type",
                table: "Histories",
                column: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Histories_Type",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Histories");

            migrationBuilder.AddColumn<bool>(
                name: "ShowToUser",
                table: "Histories",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
