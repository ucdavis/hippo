using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Core.Extensions;

public static class ClusterExtensions
{
    public static async Task<Cluster> ToCluster(this ClusterModel model, AppDbContext dbContext)
    {
        return new Cluster
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            SshName = model.SshName,
            SshKeyId = model.SshKeyId,
            SshUrl = model.SshUrl,
            IsActive = model.IsActive,
            Domain = model.Domain,
            Email = model.Email,
            AccessTypes = model.AccessTypes.Any() 
                ? await dbContext.AccessTypes.Where(at => model.AccessTypes.Contains(at.Name)).ToListAsync() 
                : new()
        };

    }
}
