using System.Text.Json;

namespace Hippo.Core.Utilities;

public static class JsonHelper
{
    private static JsonSerializerOptions _serializeOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static JsonSerializerOptions _deserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public static JsonElement ConvertToJsonElement<T>(T value)
    {
        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(value, _serializeOptions), _deserializeOptions);
    }

    public static JsonElement DeserializeToJsonElement(string value)
    {
        return JsonSerializer.Deserialize<JsonElement>(value, _deserializeOptions);
    }

    public static T ConvertFromJsonElement<T>(JsonElement? value) where T : class
    {
        return value?.Deserialize<T>(_deserializeOptions);
    }
}