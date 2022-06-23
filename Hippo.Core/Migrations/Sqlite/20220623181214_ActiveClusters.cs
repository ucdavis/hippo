using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hippo.Core.Migrations.Sqlite
{
    public partial class ActiveClusters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsInactive",
                table: "Clusters",
                newName: "IsActive"
            );

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Clusters",
                type: "INTEGER",
                nullable: false,
                defaultValue: true
            );

            // toggle boolean of any existing values (the Sqlite way)
            migrationBuilder.Sql("UPDATE [Clusters] SET IsActive = ((IsActive | 1) - (IsActive & 1))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // toggle boolean of any existing values (the Sqlite way)
            migrationBuilder.Sql("UPDATE [Clusters] SET IsActive = ((IsActive | 1) - (IsActive & 1))");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Clusters",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Clusters",
                newName: "IsInactive"
            );


        }
    }
}
