using System;
using System.Linq;
using Hippo.Core.Data;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Hippo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Web.Controllers
{
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public class ClusterAdminController : Controller
    {
        private readonly ISecretsService _secretsService;
        private readonly AppDbContext _dbContext;

        public ClusterAdminController(ISecretsService secretsService, AppDbContext dbContext)
        {
            _secretsService = secretsService;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View("React");
        }

        [HttpGet]
        public async Task<IActionResult> Clusters()
        {
            var currentSecretKeyIds = new HashSet<string>(await _secretsService.GetSecretNames());

            var clusterModels = (await _dbContext.Clusters
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToArrayAsync())
                .Select(c => new ClusterModel(c, string.Empty))
                .ToArray();

            return Ok(clusterModels);
        }

        [HttpGet]
        public async Task<IActionResult> Cluster(int id)
        {
            var cluster = await _dbContext.Clusters.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            if (cluster == null)
            {
                return NotFound();
            }
            var clusterModel = new ClusterModel(cluster, string.Empty);
            return Ok(clusterModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClusterModel clusterModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrWhiteSpace(clusterModel.SshKey))
            {

                if (string.IsNullOrWhiteSpace(clusterModel.Cluster.SshKeyId))
                {
                    if (!clusterModel.SshKey.IsValidSshKey())
                    {
                        return BadRequest("Invalid SSH Key");
                    }
                    clusterModel.Cluster.SshKeyId = Guid.NewGuid().ToString();
                }
            }

            _dbContext.Clusters.Add(clusterModel.Cluster);
            await _dbContext.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(clusterModel.SshKey) && !string.IsNullOrWhiteSpace(clusterModel.Cluster.SshKeyId))
            {
                await _secretsService.SetSecret(clusterModel.Cluster.SshKeyId, clusterModel.SshKey);
            }

            return Ok(clusterModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] ClusterModel clusterModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrWhiteSpace(clusterModel.SshKey) && string.IsNullOrWhiteSpace(clusterModel.Cluster.SshKeyId))
            {
                if (!clusterModel.SshKey.IsValidSshKey())
                {
                    return BadRequest("Invalid SSH Key");
                }
                clusterModel.Cluster.SshKeyId = Guid.NewGuid().ToString();
            }

            _dbContext.Clusters.Update(clusterModel.Cluster);
            await _dbContext.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(clusterModel.SshKey))
            {
                if (!string.IsNullOrWhiteSpace(clusterModel.Cluster.SshKeyId))
                {
                    await _secretsService.SetSecret(clusterModel.Cluster.SshKeyId, clusterModel.SshKey);
                }
            }

            return Ok(clusterModel);
        }

    }
}
