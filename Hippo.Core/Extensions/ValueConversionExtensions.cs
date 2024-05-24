using System.Text.Json;
using Hippo.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hippo.Core.Extensions;

public static class ValueConversionExtensions
{
    private static JsonSerializerOptions _serializeOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static JsonSerializerOptions _deserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder, DbContext dbContext)
    {
        ValueConverter<T, string> converter = new ValueConverter<T, string>
        (
            v => EqualityComparer<T>.Default.Equals(v, default) ? "" : JsonSerializer.Serialize(v, _serializeOptions),
            v => string.IsNullOrWhiteSpace(v) ? default : JsonSerializer.Deserialize<T>(v, _deserializeOptions)
        );

        // ValueComparer ensures the client-side value plays nicely with the change tracker. The Serialize/Deserialize stuff might cause
        // performance issues when a given DbSet holds a lot of records in the change tracker. But we generally don't perform updates or 
        // change tracking on large data sets.
        ValueComparer<T> comparer = new ValueComparer<T>
        (
            (l, r) => JsonSerializer.Serialize(l, _serializeOptions) == JsonSerializer.Serialize(r, _serializeOptions),
            v => v == null ? 0 : JsonSerializer.Serialize(v, _serializeOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, _serializeOptions), _deserializeOptions)
        );

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);

        // Not sure if there is a better way to determine what column type to use
        if (dbContext.Database.IsSqlite())
            propertyBuilder.HasColumnType("TEXT");
        else
            propertyBuilder.HasColumnType("nvarchar(max)");

        return propertyBuilder;
    }
}