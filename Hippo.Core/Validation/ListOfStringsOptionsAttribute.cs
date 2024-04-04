using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Hippo.Core.Validation;

/// <summary>
/// Validates that all elements of a list of strings matches one of the options specified in a regular expression
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class ListOfStringsOptionsAttribute : RegularExpressionAttribute
{
    private readonly bool _nonEmpty;

    public ListOfStringsOptionsAttribute(string pattern, bool nonEmpty = false)
        : base(pattern)
    {
        _nonEmpty = nonEmpty;
    }

    public override bool IsValid(object value)
    {
        if (value is not IEnumerable<string> list)
            return false;

        if (_nonEmpty && !list.Any())
            return false;

        foreach (var val in list)
        {
            if (!Regex.IsMatch(val, Pattern))
                return false;
        }

        return true;
    }

    public override string FormatErrorMessage(string name)
    {
        // not sure if there is a better way than just assuming regex is a simple |-separated list
        var values = Pattern.Split("|");
        if (_nonEmpty)
            return $"The field {name} may only contain one or more of the following values ({string.Join(", ", values)})";
        else
            return $"The field {name} may only contain the following values ({string.Join(", ", values)})";
    }
}