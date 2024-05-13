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

        [HttpPost]  
        public async Task<IActionResult> CreateOrder([FromBody] Order model)
        {
            var cluster = await _dbContext.Clusters.FirstAsync(a => a.Name == Cluster);
            User principalInvestigator = null;
            //If this is created by an admin, we will use the passed PrincipalInvestigatorId, otherwise it is who created it.
            if (User.IsInRole(AccessCodes.ClusterAdminAccess))
            {
                principalInvestigator = await _dbContext.Users.FirstAsync(a => a.Id == model.PrincipalInvestigatorId);
            }
            else
            {
                principalInvestigator = await _dbContext.Users.FirstAsync(a => a.Email == User.Identity.Name); //TODO: Check if this is how we do it.
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid");
            }

            var order = new Order
            {
                Category = model.Category,
                Name = model.Name,
                Description = model.Description,
                ExternalReference = model.ExternalReference,
                Units = model.Units,
                UnitPrice = model.UnitPrice,
                Installments = model.Installments,
                Quantity = model.Quantity,
                
                //Adjustment = model.Adjustment,
                //AdjustmentReason = model.AdjustmentReason,
                SubTotal = model.Quantity * model.UnitPrice,
                Total = model.Quantity * model.UnitPrice,
                BalanceRemaining = model.Quantity * model.UnitPrice,
                Notes = model.Notes,
                AdminNotes = model.AdminNotes,
                Status = Order.Statuses.Created,
                Cluster = cluster,
                PrincipalInvestigator = principalInvestigator,
                CreatedOn = DateTime.UtcNow
            };
            // Deal with OrderMeta data
            foreach (var metaData in model.MetaData)
            {
                order.AddMetaData(metaData.Name, metaData.Value);
            }


            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            return Ok(order);
        }

    }
}
