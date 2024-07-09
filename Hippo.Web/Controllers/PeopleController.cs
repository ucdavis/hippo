using System.Threading.Tasks;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Services;
using Hippo.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class PeopleController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;

        public PeopleController(AppDbContext dbContext, IIdentityService identityService)
        {
            this._dbContext = dbContext;
            this._identityService = identityService;
        }

        // Search people based on kerb or email
        [HttpGet]
        public async Task<ActionResult> Search(string query)
        {
            User user;

            if (query.Contains('@'))
            {
                user = await _identityService.GetByEmail(query);
            }
            else
            {
                user = await _identityService.GetByKerberos(query);
            }

            return Ok(user);
        }
    }
}