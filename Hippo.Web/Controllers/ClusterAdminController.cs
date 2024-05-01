using System;
using System.Linq;
using Hippo.Core.Data;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Hippo.Core.Extensions;
using Hippo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hippo.Core.Domain;

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

            var clusterModels = await _dbContext.Clusters
                .OrderBy(c => c.Name)
                .Select(ClusterModel.Projection)
                .ToArrayAsync();

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
        public async Task<IActionResult> Delete(int id)
        {
            var cluster = await _dbContext.Clusters.SingleOrDefaultAsync(c => c.Id == id);
            if (cluster == null)
            {
                return NotFound();
            }
            cluster.IsActive = false;
            await _dbContext.SaveChangesAsync();
            return Ok();
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

                if (string.IsNullOrWhiteSpace(clusterModel.SshKeyId))
                {
                    if (!clusterModel.SshKey.IsValidSshKey())
                    {
                        return BadRequest("Invalid SSH Key");
                    }
                    clusterModel.SshKeyId = Guid.NewGuid().ToString();
                }
            }

            var cluster = await clusterModel.ToCluster(_dbContext);

            await _dbContext.Clusters.AddAsync(cluster);
            await _dbContext.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(clusterModel.SshKey) && !string.IsNullOrWhiteSpace(clusterModel.SshKeyId))
            {
                await _secretsService.SetSecret(clusterModel.SshKeyId, clusterModel.SshKey);
            }

            clusterModel.Id = cluster.Id;

            return Ok(clusterModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] ClusterModel clusterModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrWhiteSpace(clusterModel.SshKey) && string.IsNullOrWhiteSpace(clusterModel.SshKeyId))
            {
                if (!clusterModel.SshKey.IsValidSshKey())
                {
                    return BadRequest("Invalid SSH Key");
                }
                clusterModel.SshKeyId = Guid.NewGuid().ToString();
            }

            var cluster = await _dbContext.Clusters
                .Include(c => c.AccessTypes)
                .Where(c => c.Id == clusterModel.Id)
                .SingleOrDefaultAsync();

            if (cluster == null)
            {
                return NotFound();
            }

            //sync cluster.AccessTypes with clusterModel.AccessTypes
            var accessTypes = await _dbContext.AccessTypes
                .Where(at => clusterModel.AccessTypes.Contains(at.Name))
                .ToListAsync();
            cluster.AccessTypes = accessTypes;

            _dbContext.Clusters.Update(cluster);
            await _dbContext.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(clusterModel.SshKey))
            {
                if (!string.IsNullOrWhiteSpace(clusterModel.SshKeyId))
                {
                    await _secretsService.SetSecret(clusterModel.SshKeyId, clusterModel.SshKey);
                }
            }

            return Ok(clusterModel);
        }

        public async Task<IActionResult> UpdateFinancialDetails(int id, [FromBody] FinancialDetail model)
        {
            //Possibly use the secret service to set the FinancialSystemApiKey
            var cluster = await _dbContext.Clusters.SingleAsync(c => c.Id == id);
            var existingFinancialDetail = await _dbContext.FinancialDetails.SingleOrDefaultAsync(fd => fd.ClusterId == id);
            if (existingFinancialDetail == null)
            {
                existingFinancialDetail = new FinancialDetail
                {
                    ClusterId = id
                };
            }
            await _secretsService.SetSecret($"FinancialApiKeyCluster{cluster.Id}", model.FinancialSystemApiKey);
            //var xxx = await _secretsService.GetSecret($"FinancialApiKeyCluster{cluster.Id}");
            //existingFinancialDetail.FinancialSystemApiKey = model.FinancialSystemApiKey;
            existingFinancialDetail.FinancialSystemApiSource = model.FinancialSystemApiSource;
            existingFinancialDetail.ChartString = model.ChartString;
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
            return Ok(existingFinancialDetail);

        }

    }
}
