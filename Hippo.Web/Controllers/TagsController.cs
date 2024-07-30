using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Models;
using Hippo.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Serilog;
using Hippo.Core.Models;
using AccountRequest = Hippo.Core.Domain.Request;
using Hippo.Core.Extensions;
using System.Text.Json;

namespace Hippo.Web.Controllers;

[Authorize]
public class TagsController : SuperController
{
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;

    public TagsController(AppDbContext dbContext, IUserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }

        return Ok(await _dbContext.Tags
            .Where(t => t.Cluster.Name == Cluster)
            .Select(t => t.Name)
            .ToArrayAsync());
    }

    [HttpPost]
    [Authorize(AccessCodes.GroupAdminAccess)]
    public async Task<ActionResult> UpdateAccountTags([FromBody] AccountTagsModel accountTagsModel)
    {
        if (string.IsNullOrWhiteSpace(Cluster))
        {
            return BadRequest("Cluster is required");
        }

        if (accountTagsModel == null)
        {
            return BadRequest("AccountTagsModel is required");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var account = await _dbContext.Accounts
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == accountTagsModel.AccountId);
        if (account == null)
        {
            return NotFound("Account not found");
        }

        var permissions = await _userService.GetCurrentPermissionsAsync();
        var canEditTags = permissions.IsClusterOrSystemAdmin(Cluster);

        var currentUser = await _userService.GetCurrentUser();
        if (!canEditTags)
        {
            // check if user is a PI/Sponsor for given account
            canEditTags = await _dbContext.Accounts.AnyAsync(
            a => a.Cluster.Name == Cluster
            && a.OwnerId == currentUser.Id
            && a.AdminOfGroups.Any(
                g => g.MemberAccounts.Any(
                    ma => ma.Id == accountTagsModel.AccountId)));
        }

        if (!canEditTags)
        {
            return Unauthorized();
        }

        var existingTags = await _dbContext.Tags
            .Where(t => t.Cluster.Name == Cluster && accountTagsModel.Tags.Contains(t.Name))
            .ToArrayAsync();
        var createTags = accountTagsModel.Tags
            .Except(existingTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase)
            .Select(t => new Tag
            {
                Name = t,
                ClusterId = account.ClusterId
            })
            .ToArray();

        // set desired state of account tags and le change tracker determine what to do.
        account.Tags.Clear();
        account.Tags.AddRange(existingTags);
        account.Tags.AddRange(createTags);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

}
