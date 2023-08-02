﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class PuppetGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PuppetGroupsPuppetUsers",
                columns: table => new
                {
                    GroupName = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    UserKerberos = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuppetGroupsPuppetUsers", x => new { x.GroupName, x.UserKerberos });
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuppetGroupsPuppetUsers_UserKerberos",
                table: "PuppetGroupsPuppetUsers",
                column: "UserKerberos");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuppetGroupsPuppetUsers");
        }
    }
}
