using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class RefactorAccountSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TempKerberos",
                table: "TempKerberos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TempGroups",
                table: "TempGroups");

            migrationBuilder.AddColumn<int>(
                name: "ClusterId",
                table: "TempKerberos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClusterId",
                table: "TempGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ClusterId",
                table: "Histories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TempKerberos",
                table: "TempKerberos",
                columns: new[] { "ClusterId", "Kerberos" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TempGroups",
                table: "TempGroups",
                columns: new[] { "ClusterId", "Group" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TempKerberos",
                table: "TempKerberos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TempGroups",
                table: "TempGroups");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "TempKerberos");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "TempGroups");

            migrationBuilder.Sql("DELETE FROM Histories WHERE ClusterId IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "ClusterId",
                table: "Histories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TempKerberos",
                table: "TempKerberos",
                column: "Kerberos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TempGroups",
                table: "TempGroups",
                column: "Group");
        }
    }
}
