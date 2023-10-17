using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using YamlDotNet.Serialization;

namespace Hippo.Web.Services
{
    public interface IYamlService
    {
        string Get(Request request);
    }

    public class YamlService : IYamlService
    {
        public AppDbContext _dbContext { get; }

        public YamlService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }



        public string Get(Request request)
        {
            if (request.Group == null)
            {
                throw new InvalidOperationException($"Group is required");
            }

            if (request.Account == null)
            {
                throw new InvalidOperationException($"Account is required");
            }

            if (request.Account.Owner == null)
            {
                throw new InvalidOperationException($"Account Owner is required");
            }

            if (request.Account.Cluster == null)
            {
                throw new InvalidOperationException($"Cluster is required");
            }

            var yaml = new Serializer();

            return yaml.Serialize(
                new
                {
                    groups = new[] { request.Group.Name },
                    account = new
                    {
                        name = request.Account.Owner.Name,
                        email = request.Account.Owner.Email,
                        kerb = request.Account.Owner.Kerberos,
                        iam = request.Account.Owner.Iam,
                        mothra = request.Account.Owner.MothraId,
                        key = request.Account.AccountYaml
                    },
                    meta = new
                    {
                        cluster = request.Account.Cluster.Name,
                    }
                }
            );
        }
    }
}
