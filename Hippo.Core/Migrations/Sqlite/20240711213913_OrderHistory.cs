using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class OrderHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billings_Orders_OrderId",
                table: "Billings");

            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Orders_OrderId",
                table: "Histories");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaData_Orders_OrderId",
                table: "MetaData");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Accounts_PrincipalInvestigatorId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.AddForeignKey(
                name: "FK_Billings_Orders_OrderId",
                table: "Billings",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Orders_OrderId",
                table: "Histories",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaData_Orders_OrderId",
                table: "MetaData",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Accounts_PrincipalInvestigatorId",
                table: "Orders",
                column: "PrincipalInvestigatorId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billings_Orders_OrderId",
                table: "Billings");

            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Orders_OrderId",
                table: "Histories");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaData_Orders_OrderId",
                table: "MetaData");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Accounts_PrincipalInvestigatorId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.AddForeignKey(
                name: "FK_Billings_Orders_OrderId",
                table: "Billings",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Orders_OrderId",
                table: "Histories",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MetaData_Orders_OrderId",
                table: "MetaData",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Accounts_PrincipalInvestigatorId",
                table: "Orders",
                column: "PrincipalInvestigatorId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
