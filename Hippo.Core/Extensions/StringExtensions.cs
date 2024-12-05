using System.Text;
using System.Text.RegularExpressions;

public static class StringExtensions
{
    public static string SplitCamelCase(this string str)
    {
        return Regex.Replace(str, "([a-z])([A-Z])", "$1 $2", RegexOptions.Compiled);
    }

    /// <summary>
    /// Validates a public ssh key by checking if the base64-encoded key header matches the declared key type.
    /// </summary>
    public static bool IsValidSshKey(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        var m = Regex.Match(str, @"(?<type>[a-z0-9_-]+)\s+(?<key>[A-Za-z0-9+\/]+=*)(\s+(?<comment>.*)\s*)?");

        if (!m.Success)
        {
            return false;
        }

        // calculate expected key header
        var typeBytes = Encoding.ASCII.GetBytes(m.Groups["type"].Value);
        var keyHeaderBytes = new byte[] { 0, 0, 0, (byte)typeBytes.Length }.Concat(typeBytes).ToArray();
        var keyHeaderBase64 = Convert.ToBase64String(keyHeaderBytes).TrimEnd('=');

        // check if key header matches
        return m.Groups["key"].Value.StartsWith(keyHeaderBase64);
    }

    public static string EncodeBase64(this string value)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(valueBytes);
    }

    public static string DecodeBase64(this string value)
    {
        var valueBytes = Convert.FromBase64String(value);
        return Encoding.UTF8.GetString(valueBytes);
    }

    // Naive utility for converting Serilog templates and arguments to a string
    public static string FormatTemplate(this string messageTemplate, object parameter, params object[] additionalParameters)
    {
        return FormatTemplate(messageTemplate, new[] { parameter }.Concat(additionalParameters));
    }

    // Naive utility for converting Serilog templates and arguments to a string
    public static string FormatTemplate(this string messageTemplate, IEnumerable<object> parameters)
    {
        var objects = parameters as object[] ?? parameters.ToArray();

        if (objects.Length == 0)
        {
            return messageTemplate;
        }

        if (objects.Length != Regex.Matches(messageTemplate, "{.*?}").Count)
        {
            throw new ArgumentException("Number of arguments does not match number of template parameters");
        }

        var i = 0;
        return Regex.Replace(messageTemplate, "{.*?}", _ => objects[i++]?.ToString() ?? string.Empty);
    }
    public static string SafeTruncate(this string value, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= max)
        {
            return value;
        }

        if (max <= 0)
        {
            return String.Empty;
        }

        return value.Substring(0, max);
    }
}