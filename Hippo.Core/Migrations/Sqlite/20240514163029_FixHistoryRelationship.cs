using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class FixHistoryRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Orders_OrderId",
                table: "Histories");

            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Orders_OrderId1",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Histories_OrderId1",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "OrderId1",
                table: "Histories");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Orders_OrderId",
                table: "Histories",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Orders_OrderId",
                table: "Histories");

            migrationBuilder.AddColumn<int>(
                name: "OrderId1",
                table: "Histories",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Histories_OrderId1",
                table: "Histories",
                column: "OrderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Orders_OrderId",
                table: "Histories",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Orders_OrderId1",
                table: "Histories",
                column: "OrderId1",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
