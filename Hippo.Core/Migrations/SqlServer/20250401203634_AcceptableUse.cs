using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
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
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptableUsePolicyUrl",
                table: "Clusters",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptableUsePolicyAgreedOn",
                table: "Accounts",
                type: "datetime2",
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
