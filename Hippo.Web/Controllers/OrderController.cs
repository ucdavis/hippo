using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Web.Controllers
{

    [Authorize]
    public class OrderController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;


        public OrderController(AppDbContext dbContext, IAggieEnterpriseService aggieEnterpriseService)
        {
            _dbContext = dbContext;
            _aggieEnterpriseService = aggieEnterpriseService;
        }



        [HttpGet]
        [Route("api/order/validateChartString/{chartString}")]
        public async Task<ChartStringValidationModel> ValidateChartString(string chartString)
        {
            return await _aggieEnterpriseService.IsChartStringValid(chartString);
            
        }

    }
}
