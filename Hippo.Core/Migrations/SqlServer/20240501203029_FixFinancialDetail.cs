using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class FixFinancialDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChartString",
                table: "FinancialDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialSystemApiKey",
                table: "FinancialDetails",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialSystemApiSource",
                table: "FinancialDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChartString",
                table: "FinancialDetails");

            migrationBuilder.DropColumn(
                name: "FinancialSystemApiKey",
                table: "FinancialDetails");

            migrationBuilder.DropColumn(
                name: "FinancialSystemApiSource",
                table: "FinancialDetails");
        }
    }
}
