using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Models.OrderModels;
using Hippo.Core.Services;
using Hippo.Web.Models.OrderModels;
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

            var currentUserAccount = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.Cluster.Name == Cluster && a.OwnerId == currentUser.Id);
            if(currentUserAccount == null)
            {
                return Ok(new OrderListModel[0]);
            }

            var model = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && a.PrincipalInvestigatorId == currentUserAccount.Id).Select(OrderListModel.Projection()).ToListAsync(); //Filters out inactive orders
            
            return Ok(model);

            //TODO: Need to create a page for this.
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            //TODO: When accounts has an IsActive flag, we will need to ignore the query filters.

            var model = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && a.Id == id)
                .Include(a => a.MetaData).Include(a => a.Payments).Include(a => a.PrincipalInvestigator).ThenInclude(a => a.Owner)
                .Include(a => a.History.Where(w => w.Type == History.HistoryTypes.Primary)).ThenInclude(a => a.ActedBy)
                .Select(OrderDetailModel.Projection())
                .SingleOrDefaultAsync(); 
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var model = await _dbContext.Products.Where(a => a.Cluster.Name == Cluster && a.Id == id)

                .Select(OrderDetailModel.ProductProjection())
                .SingleOrDefaultAsync();
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetClusterUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Ok(null);
            }

            id = id.Trim().ToLower();

            var model = await _dbContext.Accounts.Where(a => a.Cluster.Name == Cluster && (a.Kerberos == id || a.Email == id)).Include(a => a.AdminOfGroups).ThenInclude(a => a.Cluster)
                .Include(a => a.Owner).FirstOrDefaultAsync();

            if(model == null || model.AdminOfGroups == null || !model.AdminOfGroups.Where(a => a.Cluster.Name == Cluster).Any())
            {
                return Ok(new Account());
            }

            return Ok(model);
                
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] OrderPostModel model) //TODO: Might need to change this to a post model....
        {
            //Pass the product id too? 
            var cluster = await _dbContext.Clusters.FirstAsync(a => a.Name == Cluster);

            Account? principalInvestigator = null;

            var orderToReturn = new Order();


            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            var currentAccount = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Cluster.Name == Cluster && a.OwnerId == currentUser.Id);

            //If this is created by an admin, we will use the passed PrincipalInvestigatorId, otherwise it is who created it.
            if (isClusterOrSystemAdmin && !string.IsNullOrWhiteSpace(model.PILookup))
            {
                //TODO: check if the PI is in the cluster and if they have a PI role (Add that info to GetClusterUser())

                principalInvestigator = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Cluster.Name == Cluster && (a.Email == model.PILookup || a.Kerberos == model.PILookup));
                if (principalInvestigator == null)
                {
                    principalInvestigator = currentAccount;
                }
            }
            else
            {
                principalInvestigator = currentAccount;
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid");
            }

            if(model.Id == 0)
            {
                //Ok, this is a new order that we have to create
                var order = new Order
                {
                    Category = model.Category,
                    Name = model.Name ?? model.ProductName,
                    ProductName = model.ProductName,
                    Description = model.Description,
                    ExternalReference = model.ExternalReference,
                    Units = model.Units,
                    UnitPrice = model.UnitPrice,
                    Installments = model.Installments,
                    InstallmentType = model.InstallmentType,
                    LifeCycle = model.LifeCycle,
                    Quantity = model.Quantity,
                    Billings = new List<Billing>(),

                    //Adjustment = model.Adjustment,
                    //AdjustmentReason = model.AdjustmentReason,
                    SubTotal = model.Quantity * model.UnitPrice,
                    Total = model.Quantity * model.UnitPrice,
                    BalanceRemaining = model.Quantity * model.UnitPrice,
                    Notes = model.Notes,
                    AdminNotes = model.AdminNotes,
                    Status = Order.Statuses.Created,
                    Cluster = cluster,
                    ClusterId = cluster.Id,
                    PrincipalInvestigator = principalInvestigator,
                    CreatedOn = DateTime.UtcNow
                };
                // Deal with OrderMeta data
                foreach (var metaData in model.MetaData)
                {
                    order.AddMetaData(metaData.Name, metaData.Value);
                }

                await _dbContext.Orders.AddAsync(order);

                await _historyService.OrderCreated(order, currentUser);
                await _historyService.OrderSnapshot(order, currentUser, History.OrderActions.Created);

                orderToReturn = order;

            }
            else
            {
                //Updating an existing order without changing the status.
                var existingOrder = await _dbContext.Orders.FirstAsync(a => a.Id == model.Id);
                await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Updated); //Before Changes
                if(User.IsInRole(AccessCodes.ClusterAdminAccess))
                {
                    //TODO: Check the status to limit what can be changed
                    existingOrder.Category = model.Category;
                    existingOrder.ProductName = model.ProductName;
                    existingOrder.Description = model.Description;
                    existingOrder.Adjustment = model.Adjustment;
                    existingOrder.AdjustmentReason = model.AdjustmentReason;
                    existingOrder.AdminNotes = model.AdminNotes;
                    existingOrder.InstallmentType = model.InstallmentType == Product.InstallmentTypes.Yearly ? Product.InstallmentTypes.Yearly : Product.InstallmentTypes.Monthly;
                    existingOrder.Installments = model.Installments;
                    existingOrder.UnitPrice = model.UnitPrice;
                    existingOrder.Units = model.Units;
                    existingOrder.ExternalReference = model.ExternalReference;
                    existingOrder.LifeCycle = model.LifeCycle;
                    existingOrder.ExpirationDate = model.ExpirationDate;
                    existingOrder.InstallmentDate = model.InstallmentDate;
                }
                existingOrder.Description = model.Description;
                existingOrder.Name = model.Name;
                existingOrder.Notes = model.Notes;
                if(existingOrder.Status == Order.Statuses.Created)
                {
                    existingOrder.Quantity = model.Quantity;
                }
                
                //Deal with OrderMeta data (Test this)
                foreach (var metaData in existingOrder.MetaData)
                {
                    if(metaData != null && model.MetaData.Any(a => a.Name == metaData.Name) && model.MetaData.Any(a => a.Value == metaData.Value))
                    {
                        //Keep it
                    }
                    else
                    {
                        existingOrder.MetaData.Remove(metaData);
                    }
                }
                foreach (var metaData in model.MetaData)
                {
                    if (existingOrder.MetaData.Any(a => a.Name == metaData.Name) && existingOrder.MetaData.Any(a => a.Value == metaData.Value))
                    {
                        //Nothing to do, it is already there
                    }
                    else
                    {
                        existingOrder.AddMetaData(metaData.Name, metaData.Value);
                    }
                }

                await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Updated); //After Changes
                await _historyService.OrderUpdated(existingOrder, currentUser);

                orderToReturn = existingOrder;

            }            



            await _dbContext.SaveChangesAsync();


            return Ok(orderToReturn);
        }


    }
}
