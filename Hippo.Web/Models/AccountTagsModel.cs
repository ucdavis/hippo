namespace Hippo.Web.Models;

public class AccountTagsModel
{
    public int AccountId { get; set; }
    public List<string> Tags { get; set; } = new();
}