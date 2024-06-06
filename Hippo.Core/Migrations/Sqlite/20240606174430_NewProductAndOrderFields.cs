using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class NewProductAndOrderFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LifeCycle",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InstallmentDate",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LifeCycle",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LifeCycle",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InstallmentDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LifeCycle",
                table: "Orders");
        }
    }
}
