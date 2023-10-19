using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.SqlServer
{
    public partial class RestoreSshKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccountYaml",
                table: "Accounts",
                newName: "SshKey");

            // extract ssh key from the yaml
            migrationBuilder.Sql(@"
                UPDATE [dbo].[Accounts]
                SET [SshKey] = TRIM('""' FROM SUBSTRING(
                    SshKey,
                    PATINDEX('%ssh-%', SshKey),
                    -- len is index of next newline minus index of 'ssh-' minus 1
                    CHARINDEX(CHAR(10), SshKey, PATINDEX('%ssh-%', SshKey)) - PATINDEX('%ssh-%', SshKey) - 1
                ))
                WHERE [SshKey] LIKE '%ssh-%'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SshKey",
                table: "Accounts",
                newName: "AccountYaml");
        }
    }
}
