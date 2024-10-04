using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Hippo.Core.Validation;

/// <summary>
/// Validates that all elements of a list are email addresses
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class ListOfEmailAddressAttribute : ValidationAttribute
{
    private readonly bool _nonEmpty;
    private readonly EmailAddressAttribute _emailAddressAttribute;

    public ListOfEmailAddressAttribute(bool nonEmpty = false)
        : base()
    {
        _nonEmpty = nonEmpty;
        _emailAddressAttribute = new EmailAddressAttribute();
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var list = value as IList<string>;
        if (_nonEmpty && (list == null || !list.Any()))
            return new ValidationResult($"'{validationContext.MemberName}' requires at least 1 email address");

        if (list == null)
            return ValidationResult.Success;

        foreach (var email in list)
        {
            if (!_emailAddressAttribute.IsValid(email))
                return new ValidationResult($"'{validationContext.MemberName}' contains an invalid email address.");
        }

        return ValidationResult.Success;
    }
}