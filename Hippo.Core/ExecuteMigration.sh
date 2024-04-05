if [ -z "$1" ]; then
  dotnet ef database update --startup-project ../Hippo.Web/Hippo.Web.csproj --context AppDbContextSqlServer
else
  dotnet ef database update $1 --startup-project ../Hippo.Web/Hippo.Web.csproj --context AppDbContextSqlServer
fi
# dotnet ef database update --startup-project ../Hippo.Web/Hippo.Web.csproj --context AppDbContextSqlite
# usage from PM console in the Hippo.Core directory: ./ExecuteMigration.sh <migration name or blank for latest migration>

echo 'All done';