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

    [HttpGet]
    public async Task<IActionResult> UntrackedGroupNames()
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("You must supply a cluster name.");
        }

        var groups = await _dbContext.PuppetGroupsPuppetUsers
            .Where(pgpu =>
                pgpu.ClusterName == Cluster
                && !_dbContext.Groups.Any(g =>
                    g.IsActive
                    && g.Cluster.Name == Cluster
                    && g.Name == pgpu.GroupName))
            .Select(pgpu => pgpu.GroupName)
            .Distinct()
            .OrderBy(g => g)
            .ToArrayAsync();

        return Ok(groups);
    }

    [Authorize(Policy = AccessCodes.ClusterAdminAccess)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Group group)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("You must supply a cluster name.");
        }

        if (group.Id != 0)
        {
            return BadRequest("You cannot specify an Id when creating a group.");
        }

        // make sure group name is a valid puppet group
        if (!await _dbContext.PuppetGroupsPuppetUsers.AnyAsync(pgpu =>
            pgpu.ClusterName == Cluster
            && pgpu.GroupName == group.Name))
        {
            return BadRequest($"Group {group.Name} does not exist in puppet.");
        }

        group.ClusterId = await _dbContext.Clusters
            .Where(c => c.Name == Cluster)
            .Select(c => c.Id)
            .SingleAsync();

        // make sure group name is not already in use
        if (await _dbContext.Groups.AnyAsync(g =>
            g.IsActive
            && g.ClusterId == group.ClusterId
            && g.Name == group.Name))
        {
            return BadRequest($"Group {group.Name} already exists.");
        }

        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync();

        return Ok(group);
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
            .Where(g => g.IsActive && g.Cluster.Name == Cluster)
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
