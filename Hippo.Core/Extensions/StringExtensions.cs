using System.Text.RegularExpressions;

public static class StringExtensions
{
    public static string SplitCamelCase(this string str)
    {
        return Regex.Replace(str, "([a-z])([A-Z])", "$1 $2", RegexOptions.Compiled);
    }
}