using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class PuppetGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PuppetGroups",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuppetGroups", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "PuppetUsers",
                columns: table => new
                {
                    Kerberos = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuppetUsers", x => x.Kerberos);
                });

            migrationBuilder.CreateTable(
                name: "PuppetGroupsPuppetUsers",
                columns: table => new
                {
                    GroupName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserKerberos = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuppetGroupsPuppetUsers", x => new { x.GroupName, x.UserKerberos });
                    table.ForeignKey(
                        name: "FK_PuppetGroupsPuppetUsers_PuppetGroups_GroupName",
                        column: x => x.GroupName,
                        principalTable: "PuppetGroups",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PuppetGroupsPuppetUsers_PuppetUsers_UserKerberos",
                        column: x => x.UserKerberos,
                        principalTable: "PuppetUsers",
                        principalColumn: "Kerberos",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.DropTable(
                name: "PuppetGroups");

            migrationBuilder.DropTable(
                name: "PuppetUsers");
        }
    }
}
