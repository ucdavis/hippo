using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static Hippo.Core.Domain.Account;

namespace Hippo.Web.Controllers;

[Authorize(Policy = AccessCodes.ClusterAdminAccess)]
public class AdminController : SuperController
{
    private AppDbContext _dbContext;
    private IUserService _userService;
    private IIdentityService _identityService;
    private IHistoryService _historyService;
    private ISshService _sshService;
    private INotificationService _notificationService;

    public AdminController(AppDbContext dbContext, IUserService userService, IIdentityService identityService, ISshService sshService, INotificationService notificationService, IHistoryService historyService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _identityService = identityService;
        _historyService = historyService;
        _sshService = sshService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> ClusterAdmins()
    {
        // get all users with cluster admin permissions
        return Ok(await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Permissions.Any(p => p.Cluster.Name == Cluster && p.Role.Name == Role.Codes.ClusterAdmin))
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .ToArrayAsync());
    }

    [HttpPost]
    public async Task<IActionResult> AddClusterAdmin(string id)
    {

        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("You must supply either an email or kerb id to lookup.");
        }

        var cluster = await _dbContext.Clusters.SingleAsync(c => c.Name == Cluster);

        var userLookup = id.Contains("@")
                    ? await _identityService.GetByEmail(id)
                    : await _identityService.GetByKerberos(id);
        if (userLookup == null)
        {
            return BadRequest("User Not Found");
        }

        var user = await _dbContext.Users
            .Include(u => u.Permissions.Where(p => p.Cluster.Name == Cluster && p.Role.Name == Role.Codes.ClusterAdmin))
            .Where(u => u.Iam == userLookup.Iam)
            .SingleOrDefaultAsync();

        if (user == null)
        {
            user = userLookup;
            _dbContext.Users.Add(user);
        }

        if (!user.Permissions.Any())
        {
            var perm = new Permission
            {
                Cluster = await _dbContext.Clusters.SingleAsync(c => c.Name == Cluster),
                Role = await _dbContext.Roles.SingleAsync(r => r.Name == Role.Codes.ClusterAdmin),
            };
            user.Permissions.Add(perm);
            await _historyService.RoleAdded(user, perm);
        }


        await _dbContext.SaveChangesAsync();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveClusterAdmin(int id)
    {
        var permission = await _dbContext.Permissions
            .Include(p => p.Role)
            .Include(p => p.User)
            .Where(p =>
                p.Id == id
                && p.Cluster.Name == Cluster
                && p.Role.Name == Role.Codes.ClusterAdmin)
            .SingleOrDefaultAsync();

        if (permission == null)
        {
            return BadRequest("Permission not found");
        }

        if (permission.UserId == (await _userService.GetCurrentUser()).Id)
        {
            return BadRequest("Can't remove yourself");
        }

        await _historyService.RoleRemoved(permission.User, permission);
        _dbContext.Permissions.Remove(permission);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }



    [HttpGet]
    public async Task<IActionResult> GroupAdmins()
    {
        // get all users with group admin permissions
        return Ok(await _dbContext.Accounts
            .SelectMany(GroupAdminModel.ProjectFromAccount)
            .OrderBy(ga => ga.Group.Name).ThenBy(ga => ga.Account.Name)
            .ToArrayAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Groups()
    {
        // get all groups for cluster
        return Ok(await _dbContext.Groups
            .Where(g => g.Cluster.Name == Cluster)
            .OrderBy(g => g.Name)
            .Select(g => g.Name)
            .ToArrayAsync());
    }
}
