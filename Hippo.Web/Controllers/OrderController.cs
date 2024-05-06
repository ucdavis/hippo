using Hippo.Core.Models;
using Hippo.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hippo.Web.Controllers
{

    [Authorize]
    public class OrderController : SuperController
    {
        private IAggieEnterpriseService _aggieEnterpriseService;

        public OrderController(IAggieEnterpriseService aggieEnterpriseService)
        {
            _aggieEnterpriseService = aggieEnterpriseService;
        }

        [HttpGet]
        [Route("api/order/validate-chart-string/{chartString}")]
        public async Task<ChartStringValidationModel> ValidateChartString(string chartString)
        {
            return await _aggieEnterpriseService.IsChartStringValid(chartString);
            
        }

    }
}
