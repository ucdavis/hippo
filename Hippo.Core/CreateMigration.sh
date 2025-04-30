[ "$#" -eq 1 ] || { echo "1 argument required, $# provided. Useage: sh CreateMigration <MigrationName>"; exit 1; }

dotnet ef migrations add $1 --context AppDbContextSqlite --output-dir Migrations/Sqlite --startup-project ../Hippo.Web/Hippo.Web.csproj -- --provider Sqlite
dotnet ef migrations add $1 --context AppDbContextSqlServer --output-dir Migrations/SqlServer --startup-project ../Hippo.Web/Hippo.Web.csproj -- --provider SqlServer
# usage from PM console in the Hippo.Core directory: ./CreateMigration.sh <MigrationName>

echo 'All done';