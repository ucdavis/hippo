using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Hippo.Core.Validation;

/// <summary>
/// Validates that all a given string property matches one of the values specified in a regular expression
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class StringOptionsAttribute : RegularExpressionAttribute
{
    public StringOptionsAttribute(string pattern)
        : base(pattern)
    {
    }

    public override string FormatErrorMessage(string name)
    {
        // not sure if there is a better way than just assuming regex is a simple |-separated list
        var values = Pattern.Split("|");
        return $"The field {name} may only contain one of the following values ({string.Join(", ", values)})";
    }
}