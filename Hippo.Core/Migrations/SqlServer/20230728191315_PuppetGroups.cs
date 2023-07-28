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
                name: "PuppetGroupPuppetUser",
                columns: table => new
                {
                    GroupsName = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    UsersKerberos = table.Column<string>(type: "nvarchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuppetGroupPuppetUser", x => new { x.GroupsName, x.UsersKerberos });
                    table.ForeignKey(
                        name: "FK_PuppetGroupPuppetUser_PuppetGroups_GroupsName",
                        column: x => x.GroupsName,
                        principalTable: "PuppetGroups",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PuppetGroupPuppetUser_PuppetUsers_UsersKerberos",
                        column: x => x.UsersKerberos,
                        principalTable: "PuppetUsers",
                        principalColumn: "Kerberos",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuppetGroupPuppetUser_UsersKerberos",
                table: "PuppetGroupPuppetUser",
                column: "UsersKerberos");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuppetGroupPuppetUser");

            migrationBuilder.DropTable(
                name: "PuppetGroups");

            migrationBuilder.DropTable(
                name: "PuppetUsers");
        }
    }
}
