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
        private readonly IUserService _userService;
        private readonly IHistoryService _historyService;


        public OrderController(AppDbContext dbContext, IAggieEnterpriseService aggieEnterpriseService, IUserService userService, IHistoryService historyService)
        {
            _dbContext = dbContext;
            _aggieEnterpriseService = aggieEnterpriseService;
            _userService = userService;
            _historyService = historyService;
        }



        [HttpGet]
        [Route("api/order/validateChartString/{chartString}")]
        public async Task<ChartStringValidationModel> ValidateChartString(string chartString)
        {
            return await _aggieEnterpriseService.IsChartStringValid(chartString);
            
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var currentUser = await _userService.GetCurrentUser();
            var orders = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && a.PrincipalInvestigatorId == currentUser.Id).ToListAsync(); //Filters out inactive orders
            return Ok(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _dbContext.Orders.Include(a => a.MetaData).Include(a => a.Payments).Include(a => a.PrincipalInvestigator).Include(a => a.History).SingleOrDefaultAsync(a => a.Cluster.Name == Cluster && a.Id == id); 
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }


        [HttpPost]  
        public async Task<IActionResult> CreateOrder([FromBody] Order model)
        {
            var cluster = await _dbContext.Clusters.FirstAsync(a => a.Name == Cluster);
            User principalInvestigator = null;



            var currentUser = await _userService.GetCurrentUser();
            //If this is created by an admin, we will use the passed PrincipalInvestigatorId, otherwise it is who created it.
            if (User.IsInRole(AccessCodes.ClusterAdminAccess))
            {
                principalInvestigator = await _dbContext.Users.FirstAsync(a => a.Id == model.PrincipalInvestigatorId);
            }
            else
            {
                principalInvestigator = currentUser;
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

            //var history = new History
            //{
            //    Order = order,
            //    ClusterId = cluster.Id,
            //    Status = History.OrderActions.Created,
            //    ActedBy = currentUser,
            //    AdminAction = currentUser != principalInvestigator,
            //    Action = History.OrderActions.Created,
            //    Details = $"Order created by {currentUser.Email}" //Dump in json of the order? //Write if it is an adhoc order?
            //};
            //_dbContext.Histories.Add(history); //Add it here or call history service?
            await _historyService.OrderCreated(order, currentUser);

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            return Ok(order);
        }

    }
}
