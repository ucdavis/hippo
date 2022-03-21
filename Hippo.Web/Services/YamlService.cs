using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Hippo.Web.Services
{
    public interface IYamlService
    {
        Task<string> Get(User currentUser, AccountCreateModel accountCreateModel);
    }

    public class YamlService : IYamlService
    {
        public AppDbContext _dbContext { get; }

        public YamlService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        

        public async Task<string> Get(User currentUser, AccountCreateModel accountCreateModel)
        {
            var sponsorAccount = await _dbContext.Accounts.Include(a => a.Owner).SingleAsync(a => a.Id == accountCreateModel.SponsorId);

            var sb = new StringBuilder();
            sb.AppendLine("sponsor:");
            sb.AppendLine($"    accountname: {sponsorAccount.Name}");
            sb.AppendLine($"    name: {sponsorAccount.Owner.Name}");
            sb.AppendLine($"    email: {sponsorAccount.Owner.Email}");
            sb.AppendLine($"    kerb: {sponsorAccount.Owner.Kerberos}");
            sb.AppendLine($"    iam: {sponsorAccount.Owner.Iam}");
            sb.AppendLine($"    mothra: {sponsorAccount.Owner.MothraId}");
            sb.AppendLine();
            sb.AppendLine("Account:");
            sb.AppendLine($"    name: {currentUser.Name}");
            sb.AppendLine($"    email: {currentUser.Email}");
            sb.AppendLine($"    kerb: {currentUser.Kerberos}");
            sb.AppendLine($"    iam: {currentUser.Iam}");
            sb.AppendLine($"    mothra: {currentUser.MothraId}");
            sb.AppendLine($"    key: \"{accountCreateModel.SshKey}\"");

            return sb.ToString();
        }
    }
}
