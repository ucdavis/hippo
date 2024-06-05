using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class ChangePiToBeAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_PrincipalInvestigatorId",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Accounts_PrincipalInvestigatorId",
                table: "Orders",
                column: "PrincipalInvestigatorId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Accounts_PrincipalInvestigatorId",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_PrincipalInvestigatorId",
                table: "Orders",
                column: "PrincipalInvestigatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
