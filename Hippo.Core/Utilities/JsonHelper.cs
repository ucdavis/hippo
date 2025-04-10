using System.Text.Json;
using System.Text.Json.Nodes;

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

    public static JsonNode ConvertToJsonNode<T>(T value)
    {
        return JsonSerializer.Deserialize<JsonNode>(JsonSerializer.Serialize(value, _serializeOptions), _deserializeOptions);
    }

    public static JsonNode DeserializeToJsonNode(string value)
    {
        return JsonSerializer.Deserialize<JsonNode>(value, _deserializeOptions);
    }

    public static T ConvertFromJsonElement<T>(JsonElement? value) where T : class
    {
        return value?.Deserialize<T>(_deserializeOptions);
    }

    /// <summary>
    /// Ensures consistent formatting of json so that string equality works as long as
    /// JSON is semantically equivalent
    /// </summary>
    public static JsonElement NormalizeJson(JsonElement element)
    {
        var node = JsonNode.Parse(element.GetRawText());
        var normalizedNode = NormalizeNode(node);

        // Convert back to JsonElement by parsing the normalized JSON string
        using var doc = JsonDocument.Parse(normalizedNode!.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }));

        // Return a copy of the root element
        return doc.RootElement.Clone();
    }

    private static JsonNode NormalizeNode(JsonNode node)
    {
        switch (node)
        {
            case JsonObject obj:
                var sorted = new JsonObject();
                foreach (var kv in obj.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                {
                    sorted[kv.Key] = NormalizeNode(kv.Value);
                }
                return sorted;

            case JsonArray array:
                var normalizedArray = new JsonArray();
                foreach (var item in array)
                {
                    normalizedArray.Add(NormalizeNode(item));
                }
                return normalizedArray;

            default:
                return node is not null
                    ? JsonNode.Parse(node.ToJsonString())
                    : null;
        }
    }
}