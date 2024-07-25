using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
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
                type: "nvarchar(50)",
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
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
