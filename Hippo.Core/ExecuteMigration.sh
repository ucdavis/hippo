dotnet ef database update --startup-project ../Hippo.Web/Hippo.Web.csproj --context AppDbContextSqlServer
# dotnet ef database update --startup-project ../Hippo.Web/Hippo.Web.csproj --context AppDbContextSqlite
# usage from PM console in the Hippo.Core directory: ./ExecuteMigration.sh

echo 'All done';