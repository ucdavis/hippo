using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class RecurringOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IsRecurring",
                table: "Orders",
                column: "IsRecurring");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_NextPaymentDate",
                table: "Orders",
                column: "NextPaymentDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_IsRecurring",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_NextPaymentDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Orders");
        }
    }
}
