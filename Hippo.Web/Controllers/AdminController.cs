using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Models.OrderModels;
using Hippo.Core.Services;
using Hippo.Web.Extensions;
using Hippo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text;
using static Hippo.Core.Domain.Account;
using static Hippo.Core.Models.SlothModels.TransferViewModel;

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
    private ISecretsService _secretsService;
    private IAggieEnterpriseService _aggieEnterpriseService;
    private ISlothService _slothService;

    public AdminController(AppDbContext dbContext, IUserService userService, IIdentityService identityService, ISshService sshService, INotificationService notificationService, IHistoryService historyService, ISecretsService secretsService, IAggieEnterpriseService aggieEnterpriseService, ISlothService slothService)
    {
        _dbContext = dbContext;
        _userService = userService;
        _identityService = identityService;
        _historyService = historyService;
        _sshService = sshService;
        _notificationService = notificationService;
        _secretsService = secretsService;
        _aggieEnterpriseService = aggieEnterpriseService;
        _slothService = slothService;
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
    public async Task<IActionResult> Groups()
    {
        // get all groups for cluster
        return Ok(await _dbContext.Groups
            .Where(g => g.Cluster.Name == Cluster)
            .OrderBy(g => g.Name)
            .Select(g => g.Name)
            .ToArrayAsync());
    }

    [HttpGet]
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public async Task<IActionResult> FinancialDetails()
    {
        var cluster = await _dbContext.Clusters.AsNoTracking().SingleAsync(c => c.Name == Cluster);
        var existingFinancialDetail = await _dbContext.FinancialDetails.SingleOrDefaultAsync(fd => fd.ClusterId == cluster.Id);
        var clusterModel = new FinancialDetailModel
        {
            FinancialSystemApiKey = string.Empty,
            FinancialSystemApiSource = existingFinancialDetail?.FinancialSystemApiSource,
            ChartString = existingFinancialDetail?.ChartString,
            AutoApprove = existingFinancialDetail?.AutoApprove ?? false,
            MaskedApiKey = "NOT SET"
        };

        if (existingFinancialDetail != null)
        {
            var apiKey = await _secretsService.GetSecret(existingFinancialDetail.SecretAccessKey);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var sb = new StringBuilder();
                for (var i = 0; i < apiKey.Length; i++)
                {
                    if (i < 4 || i >= apiKey.Length - 4)
                    {
                        sb.Append(apiKey[i]);
                    }
                    else
                    {
                        sb.Append('*');
                    }
                }

                clusterModel.MaskedApiKey = sb.ToString();

                clusterModel.IsSlothValid = await _slothService.TestApiKey(cluster.Id);
            }
        }

       
        
        return Ok(clusterModel);
    }

    [HttpPost]
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public async Task<IActionResult> UpdateFinancialDetails([FromBody] FinancialDetailModel model)
    {
        //Possibly use the secret service to set the FinancialSystemApiKey
        var cluster = await _dbContext.Clusters.SingleAsync(c => c.Name == Cluster);
        var existingFinancialDetail = await _dbContext.FinancialDetails.SingleOrDefaultAsync(fd => fd.ClusterId == cluster.Id);
        if (existingFinancialDetail == null)
        {
            existingFinancialDetail = new FinancialDetail
            {
                ClusterId = cluster.Id,
                SecretAccessKey = Guid.NewGuid().ToString(),

            };
        }
        var validateChartString = await _aggieEnterpriseService.IsChartStringValid(model.ChartString, Directions.Credit);
        if (!validateChartString.IsValid)
        {
            return BadRequest($"Invalid Chart String Errors: {validateChartString.Message}");
        }
        if (!string.IsNullOrWhiteSpace(model.FinancialSystemApiKey))
        {
            await _secretsService.SetSecret(existingFinancialDetail.SecretAccessKey, model.FinancialSystemApiKey);
        }
        //var xxx = await _secretsService.GetSecret(existingFinancialDetail.SecretAccessKey);
        existingFinancialDetail.FinancialSystemApiSource = model.FinancialSystemApiSource;
        existingFinancialDetail.ChartString = validateChartString.ChartString;
        existingFinancialDetail.AutoApprove = model.AutoApprove;

        if (existingFinancialDetail.Id == 0)
        {
            await _dbContext.FinancialDetails.AddAsync(existingFinancialDetail);
        }
        else
        {
            _dbContext.FinancialDetails.Update(existingFinancialDetail);
        }

        await _dbContext.SaveChangesAsync();

        var clusterModel = new FinancialDetailModel
        {
            FinancialSystemApiKey = string.Empty,
            FinancialSystemApiSource = existingFinancialDetail?.FinancialSystemApiSource,
            ChartString = existingFinancialDetail?.ChartString,
            AutoApprove = existingFinancialDetail?.AutoApprove ?? false,
            MaskedApiKey = "NOT SET"
        };
        var apiKey = await _secretsService.GetSecret(existingFinancialDetail?.SecretAccessKey);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var sb = new StringBuilder();
            for (var i = 0; i < apiKey.Length; i++)
            {
                if (i < 4 || i >= apiKey.Length - 4)
                {
                    sb.Append(apiKey[i]);
                }
                else
                {
                    sb.Append('*');
                }
            }

            clusterModel.MaskedApiKey = sb.ToString();

            clusterModel.IsSlothValid = await _slothService.TestApiKey(cluster.Id);
        }

        return Ok(clusterModel);

    }
}
