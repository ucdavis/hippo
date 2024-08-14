using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class MetaDataCascadeDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetaData_Orders_OrderId",
                table: "MetaData");

            migrationBuilder.AddForeignKey(
                name: "FK_MetaData_Orders_OrderId",
                table: "MetaData",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetaData_Orders_OrderId",
                table: "MetaData");

            migrationBuilder.AddForeignKey(
                name: "FK_MetaData_Orders_OrderId",
                table: "MetaData",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
