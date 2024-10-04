using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Hippo.Core.Validation;

/// <summary>
/// Validates that all elements of a list of strings matches one of the options specified in a regular expression
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class ListOfEmailAddressAttribute : ValidationAttribute
{
    private readonly bool _nonEmpty;
    private readonly EmailAddressAttribute _emailAddressAttribute;
    private const string invalidError = "'{0}' contains an invalid email address.";
    private const string emptyError = "'{0}' requires at least 1 email address";

    public ListOfEmailAddressAttribute(bool nonEmpty = false)
        : base()
    {
        _nonEmpty = nonEmpty;
        _emailAddressAttribute = new EmailAddressAttribute();
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var list = value as IList<string>;
        if (_nonEmpty && list == null || !list.Any())
            return new ValidationResult(emptyError);

        if (list == null)
            return ValidationResult.Success;

        foreach (var email in list)
        {
            if (!_emailAddressAttribute.IsValid(email))
                return new ValidationResult(invalidError);
        }

        return ValidationResult.Success;
    }

    public override string FormatErrorMessage(string name)
    {
        return String.Format(ErrorMessageString, name);
    }
}