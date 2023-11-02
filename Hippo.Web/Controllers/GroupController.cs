using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Models;
using Hippo.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Hippo.Core.Models;
using EFCore.BulkExtensions;

namespace Hippo.Web.Controllers;

[Authorize]
public class GroupController : SuperController
{
    private readonly AppDbContext _dbContext;


    public GroupController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult> Groups()
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("You must supply a cluster name.");
        }

        var groups = await _dbContext.Groups
            .AsNoTracking()
            .Where(g => g.Cluster.Name == Cluster)
            .OrderBy(g => g.DisplayName)
            .Select(GroupModel.Projection)
            .ToArrayAsync();

        return Ok(groups);
    }

    [HttpGet]
    public async Task<ActionResult> GroupNames()
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("You must supply a cluster name.");
        }

        return Ok(await _dbContext.Groups
            .AsNoTracking()
            .Where(g => g.Cluster.Name == Cluster)
            .OrderBy(g => g.DisplayName)
            .ToArrayAsync());
    }

    [Authorize(Policy = AccessCodes.ClusterAdminAccess)]
    [HttpPost]
    public async Task<IActionResult> Update([FromBody] Group group)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("You must supply a cluster name.");
        }

        if (group.Id == 0)
        {
            return BadRequest("You must specify an Id when updating a group.");
        }

        var existingGroup = await _dbContext.Groups
            .Where(g => g.Cluster.Name == Cluster)
            .SingleOrDefaultAsync(g => g.Id == group.Id);
        if (existingGroup == null)
        {
            return BadRequest($"Group does not exist.");
        }

        existingGroup.DisplayName = group.DisplayName;
        await _dbContext.SaveChangesAsync();
        return Ok(existingGroup);
    }
}
