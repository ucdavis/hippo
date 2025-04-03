using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AcceptableUse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptableUsePolicyUpdatedOn",
                table: "Clusters",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptableUsePolicyUrl",
                table: "Clusters",
                type: "TEXT",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptableUsePolicyAgreedOn",
                table: "Accounts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptableUsePolicyUpdatedOn",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "AcceptableUsePolicyUrl",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "AcceptableUsePolicyAgreedOn",
                table: "Accounts");
        }
    }
}
