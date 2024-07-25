using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class useSecretFinancilDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinancialSystemApiKey",
                table: "FinancialDetails");

            migrationBuilder.AlterColumn<string>(
                name: "FinancialSystemApiSource",
                table: "FinancialDetails",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecretAccessKey",
                table: "FinancialDetails",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretAccessKey",
                table: "FinancialDetails");

            migrationBuilder.AlterColumn<string>(
                name: "FinancialSystemApiSource",
                table: "FinancialDetails",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "FinancialSystemApiKey",
                table: "FinancialDetails",
                type: "TEXT",
                maxLength: 128,
                nullable: true);
        }
    }
}
