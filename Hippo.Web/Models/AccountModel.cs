using System.Linq.Expressions;
using Hippo.Core.Domain;

namespace Hippo.Web.Models
{
    public class AccountModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedOn { get; set; }
        public string Cluster { get; set; } = "";
        public User? Owner { get; set; }
        public string Group { get; set; } = "";
        public DateTime UpdatedOn { get; set; }

        public AccountModel()
        {

        }

        public AccountModel(Account account)
        {
            Id = account.Id;
            Name = account.Name;
            Status = account.Status.ToString();
            CreatedOn = account.CreatedOn;
            Cluster = account.Cluster.Name;
            Owner = account.Owner;
            Group = account.Group.Name;
            UpdatedOn = account.UpdatedOn;
        }

        public static Expression<Func<Account, AccountModel>> Projection
        {
            get
            {
                return a => new AccountModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Status = a.Status.ToString(),
                    CreatedOn = a.CreatedOn,
                    Cluster = a.Cluster.Name,
                    Owner = a.Owner,
                    Group = a.Group.Name,
                    UpdatedOn = a.UpdatedOn
                };
            }
        }
    }
}