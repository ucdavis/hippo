using Hippo.Core.Data;
using Hippo.Core.Extensions;
using Hippo.Core.Models;
using Hippo.Core.Services;
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


    }
}
