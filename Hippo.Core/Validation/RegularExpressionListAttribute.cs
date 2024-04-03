using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Hippo.Core.Validation;

/// <summary>
/// Validates that all elements of a list of strings matches a regular expression
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class RegularExpressionListAttribute : RegularExpressionAttribute
{
    private readonly bool _nonEmpty;

    public RegularExpressionListAttribute(string pattern, bool nonEmpty = false)
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
}