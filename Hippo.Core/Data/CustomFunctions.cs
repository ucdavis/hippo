using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Hippo.Core.Data
{
    public static class CustomFunctions
    {
        [DbFunction("JSON_VALUE", IsBuiltIn = true)]
        public static string JsonValue(string column, [NotParameterized] string path) => throw new NotSupportedException();

        public static ModelBuilder AddCustomFunctions(this ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction(() => JsonValue(default, default));
            return modelBuilder;
        }
    }
}