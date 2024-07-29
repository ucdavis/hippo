using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using Hippo.Core.Models;
using Hippo.Core.Models.OrderModels;
using Hippo.Core.Services;
using Hippo.Web.Models.OrderModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Hippo.Core.Domain.Product;
using static Hippo.Core.Models.SlothModels.TransferViewModel;

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
        [Route("api/order/validateChartString/{chartString}/{direction}")]
        public async Task<ChartStringValidationModel> ValidateChartString(string chartString, string direction)
        {
            return await _aggieEnterpriseService.IsChartStringValid(chartString, direction);

        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var currentUser = await _userService.GetCurrentUser();

            var currentUserAccount = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.Cluster.Name == Cluster && a.OwnerId == currentUser.Id);
            if (currentUserAccount == null)
            {
                return Ok(new OrderListModel[0]);
            }

            var model = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && a.PrincipalInvestigatorId == currentUserAccount.Id).Select(OrderListModel.Projection()).ToListAsync(); //Filter out inactive orders?

            return Ok(model);
        }

        [HttpGet]
        public async Task<IActionResult> AdminOrders()
        {
            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            if (!isClusterOrSystemAdmin)
            {
                return BadRequest("You do not have permission to view this page.");
            }
            var currentUserAccount = await _dbContext.Accounts.SingleOrDefaultAsync(a => a.Cluster.Name == Cluster && a.OwnerId == currentUser.Id);
            if (currentUserAccount == null)
            {
                return Ok(new OrderListModel[0]);
            }

            //Probably will want to filter out old ones that are completed and the expiration date has passed.
            var adminStatuses = new List<string> { Order.Statuses.Submitted, Order.Statuses.Processing, Order.Statuses.Active, Order.Statuses.Completed };

            var model = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && adminStatuses.Contains(a.Status)).Select(OrderListModel.Projection()).ToListAsync(); //Filter out inactive orders?

            return Ok(model);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            //TODO: When accounts has an IsActive flag, we will need to ignore the query filters.

            var model = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && a.Id == id)
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

            if (model == null || model.AdminOfGroups == null || !model.AdminOfGroups.Where(a => a.Cluster.Name == Cluster).Any())
            {
                return Ok(new Account());
            }

            return Ok(model);

        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] OrderPostModel model) 
        {
            //Pass the product id too? 
            var cluster = await _dbContext.Clusters.FirstAsync(a => a.Name == Cluster);            

            var orderToReturn = new Order();


            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);



            var currentAccount = await _dbContext.Accounts.FirstAsync(a => a.Cluster.Name == Cluster && a.OwnerId == currentUser.Id);

            Account? principalInvestigator = currentAccount;

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
                if (!isClusterOrSystemAdmin)
                {
                    var isSponsor = await _dbContext.Accounts.Where(a => a.Cluster.Name == Cluster && a.Id == principalInvestigator.Id).Include(a => a.AdminOfGroups).ThenInclude(a => a.Cluster)
                        .Include(a => a.Owner).FirstOrDefaultAsync();

                    if (isSponsor == null || isSponsor.AdminOfGroups == null || !isSponsor.AdminOfGroups.Where(a => a.Cluster.Name == Cluster).Any())
                    {
                        return BadRequest("User not a Sponsor/PI. Unable to continue");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid");
            }

            if(!InstallmentTypes.Types.Contains(model.InstallmentType))
            {
                return BadRequest("Invalid Installment Type Detected.");
            }

            if (model.Id == 0)
            {
                var processingResult = await SaveNewOrder(model, principalInvestigator, cluster, isClusterOrSystemAdmin, currentUser);
                if (!processingResult.Success)
                {
                    return BadRequest(processingResult.Message);
                }
                orderToReturn = processingResult.Order;
            }
            else
            {
                var processingResult = await UpdateExistingOrder(model, isClusterOrSystemAdmin, currentUser);
                if (!processingResult.Success)
                {
                    return BadRequest(processingResult.Message);
                }

                orderToReturn = processingResult.Order;

            }



            await _dbContext.SaveChangesAsync();


            return Ok(orderToReturn);
        }



        [HttpPost]
        public async Task<IActionResult> UpdateBilling([FromBody] OrderPostModel model)
        {
            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            //TODO: Validation
            //Updating an existing order without changing the status.
            var existingOrder = await _dbContext.Orders.Include(a => a.PrincipalInvestigator).Include(a => a.Cluster).Include(a => a.Billings).FirstAsync(a => a.Id == model.Id);
            if (existingOrder.PrincipalInvestigator.Owner.Id != currentUser.Id && !isClusterOrSystemAdmin) //Do we want admins to be able to make these chanegs?
            {
                return BadRequest("You do not have permission to update the billing information on this order.");
            }


            await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Updated); //Before Changes

            var updateBilling = await UpdateOrderBillingInfo(existingOrder, model);
            if (!updateBilling.Success)
            {
                return BadRequest(updateBilling.Message);
            }

            await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Updated); //After Changes
            await _historyService.OrderUpdated(existingOrder, currentUser, "Billing Information Updated.");

            var orderToReturn = existingOrder;

            try
            { 
                await _dbContext.SaveChangesAsync(); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            //To make sure the model has all the info needed to update the UI
            var rtmodel = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && a.Id == model.Id)
                .Select(OrderDetailModel.Projection())
                .SingleOrDefaultAsync();

            return Ok(rtmodel);



        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string expectedStatus)
        {
            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            var existingOrder = await _dbContext.Orders.Include(a => a.PrincipalInvestigator.Owner).Include(a => a.Cluster).Include(a => a.Billings).FirstAsync(a => a.Id == id);
            var isPi = existingOrder.PrincipalInvestigator.Owner.Id == currentUser.Id;


            if (!isPi && !isClusterOrSystemAdmin)
            {
                return BadRequest("You do not have permission to change the status of this order.");
            }

            switch (existingOrder.Status)
            {
                case Order.Statuses.Created:
                    if (!isPi)
                    {
                        return BadRequest("You cannot change the status of an order in the created status. The sponsor/PI has to do this.");
                    }
                    if (expectedStatus != Order.Statuses.Submitted)
                    {
                        return BadRequest("Unexpected Status found. May have already been updated.");
                    }
                    if (existingOrder.Billings.Count == 0)
                    {
                        return BadRequest("You must have billing information to submit an order.");
                    }
                    existingOrder.Status = Order.Statuses.Submitted;
                    await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Updated);
                    await _historyService.OrderUpdated(existingOrder, currentUser, "Order Submitted.");

                    break;
                case Order.Statuses.Submitted:
                    if (!isClusterOrSystemAdmin)
                    {
                        return BadRequest("You do not have permission to change the status of this order.");
                    }
                    if (expectedStatus != Order.Statuses.Processing)
                    {
                        return BadRequest("Unexpected Status found. May have already been updated.");
                    }
                    existingOrder.Status = Order.Statuses.Processing;
                    await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Updated);
                    await _historyService.OrderUpdated(existingOrder, currentUser, "Order Processing.");

                    break;
                case Order.Statuses.Processing:
                    if (!isClusterOrSystemAdmin)
                    {
                        return BadRequest("You do not have permission to change the status of this order.");
                    }
                    if (expectedStatus != Order.Statuses.Active)
                    {
                        return BadRequest("Unexpected Status found. May have already been updated.");
                    }
                    if (existingOrder.InstallmentDate == null)
                    {
                        existingOrder.InstallmentDate = DateTime.UtcNow;
                    }
                    if (existingOrder.ExpirationDate == null)
                    {
                        existingOrder.ExpirationDate = existingOrder.ExpirationDate = existingOrder.InstallmentDate.Value.AddMonths(existingOrder.LifeCycle);
                    }

                    existingOrder.Status = Order.Statuses.Active;

                    var now = DateTime.UtcNow;
                    switch (existingOrder.InstallmentType)
                    {
                        case InstallmentTypes.Monthly:
                            existingOrder.NextPaymentDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).Date.FromPacificTime();
                            break;
                        case InstallmentTypes.Yearly:
                            existingOrder.NextPaymentDate = new DateTime(now.Year, 1, 1).AddYears(1).Date.FromPacificTime();
                            break;
                        case InstallmentTypes.OneTime:
                            existingOrder.NextPaymentDate = now.AddDays(1).ToPacificTime().Date.FromPacificTime();
                            break;
                    }



                    await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Updated);
                    await _historyService.OrderUpdated(existingOrder, currentUser, "Order Activated.");

                    break;
                default:
                    return BadRequest("You cannot change the status of an order in the current status.");
            }


            await _dbContext.SaveChangesAsync();

            //To make sure the model has all the info needed to update the UI
            var model = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && a.Id == id)
                .Select(OrderDetailModel.Projection())
                .SingleOrDefaultAsync();

            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var currentUser = await _userService.GetCurrentUser();
            //var permissions = await _userService.GetCurrentPermissionsAsync();
            //var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            var existingOrder = await _dbContext.Orders.Include(a => a.PrincipalInvestigator).Include(a => a.Cluster).FirstAsync(a => a.Id == id);
            var isPi = existingOrder.PrincipalInvestigator.Owner.Id == currentUser.Id;

            if (!isPi)
            {
                return BadRequest("You do not have permission to cancel this order.");
            }

            //TODO: Maybe only allow cancel if it is created? My thought is the admin will click approve to move to the processing status before they do anything else.
            if (existingOrder.Status != Order.Statuses.Created && existingOrder.Status != Order.Statuses.Submitted)
            {
                return BadRequest("You cannot cancel an order that is not in the Created or Submitted status.");
            }

            existingOrder.Status = Order.Statuses.Cancelled;
            await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Cancelled);
            await _historyService.OrderUpdated(existingOrder, currentUser, "Order Cancelled.");

            await _dbContext.SaveChangesAsync();

            //To make sure the model has all the info needed to update the UI
            var model = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && a.Id == id)
                .Select(OrderDetailModel.Projection())
                .SingleOrDefaultAsync();

            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            if (!isClusterOrSystemAdmin)
            {
                return BadRequest("You do not have permission to reject this order.");
            }
            var existingOrder = await _dbContext.Orders.Include(a => a.PrincipalInvestigator).Include(a => a.Cluster).FirstOrDefaultAsync(a => a.Id == id);
            if (existingOrder == null)
            {
                return NotFound();
            }
            if (existingOrder.Status != Order.Statuses.Submitted && existingOrder.Status != Order.Statuses.Processing)
            {
                return BadRequest("You cannot reject an order that is not in the Submitted or Processing status.");
            }
            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest("You must provide a reason for rejecting the order.");
            }

            existingOrder.Status = Order.Statuses.Rejected;
            await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Rejected);
            await _historyService.OrderUpdated(existingOrder, currentUser, $"Order Rejected: {reason}");

            await _dbContext.SaveChangesAsync();

            //To make sure the model has all the info needed to update the UI
            var model = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && a.Id == id)
                .Select(OrderDetailModel.Projection())
                .SingleOrDefaultAsync();

            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> MakePayment(int id, decimal amount)
        {
            var currentUser = await _userService.GetCurrentUser();
            //var permissions = await _userService.GetCurrentPermissionsAsync();
            //var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            amount = Math.Round(amount, 2);

            var order = await _dbContext.Orders.Include(a => a.PrincipalInvestigator).Include(a => a.Payments).Include(a => a.Cluster).FirstAsync(a => a.Id == id && a.Cluster.Name == Cluster);
            if (order == null)
            {
                return NotFound();
            }

            if (order.PrincipalInvestigator.Owner.Id != currentUser.Id)
            {
                return BadRequest("You do not have permission to make a payment on this order.");
            }

            if (order.Status != Order.Statuses.Active)
            {
                return BadRequest("Order must be in Active status to make a payment.");
            }

            if (amount <= 0.01m)
            {
                return BadRequest("Amount must be greater than 0.01");
            }

            var totalPayments = order.Payments.Where(a => a.Status != Payment.Statuses.Cancelled).Sum(a => a.Amount);


            if (amount > order.BalanceRemaining || amount > (order.Total - totalPayments))
            {
                return BadRequest("Amount must be less than or equal to the balance remaining including payments that have not completed.");
            }

            var payment = new Payment
            {
                Amount = amount,
                Order = order,
                Status = Payment.Statuses.Created,
                CreatedById = currentUser.Id,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = currentUser
            };

            order.Payments.Add(payment);
            order.BalanceRemaining -= amount;

            await _historyService.OrderSnapshot(order, currentUser, History.OrderActions.Updated);
            await _historyService.OrderUpdated(order, currentUser, $"Manual Payment of ${amount} made.");

            await _dbContext.SaveChangesAsync();

            var model = await _dbContext.Orders.Where(a => a.Cluster.Name == Cluster && a.Id == id)
                .Select(OrderDetailModel.Projection())
                .SingleOrDefaultAsync();

            return Ok(model);
        }

        private async Task<ProcessingResult> SaveNewOrder(OrderPostModel model, Account principalInvestigator, Cluster cluster, bool isClusterOrSystemAdmin, User currentUser)
        {
            var rtValue = new ProcessingResult();
            var nextStatus = Order.Statuses.Created;

            if(principalInvestigator.OwnerId == currentUser.Id)
            {
                nextStatus = Order.Statuses.Submitted;
                if (!model.Billings.Any())
                {
                    rtValue.Success = false;
                    rtValue.Message = "You must have billing information to submit an order.";
                    return rtValue;
                }
            }


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


                SubTotal = model.Quantity * model.UnitPrice,
                Total = model.Quantity * model.UnitPrice,
                BalanceRemaining = model.Quantity * model.UnitPrice,
                Notes = model.Notes,
                AdminNotes = model.AdminNotes,
                Status = nextStatus,
                Cluster = cluster,
                ClusterId = cluster.Id,
                PrincipalInvestigator = principalInvestigator,
                CreatedOn = DateTime.UtcNow
            };
            if (isClusterOrSystemAdmin)
            {
                if (!string.IsNullOrWhiteSpace(model.ExpirationDate))
                {
                    order.ExpirationDate = DateTime.Parse(model.ExpirationDate);
                    order.ExpirationDate = order.ExpirationDate.FromPacificTime();
                }
                if (!string.IsNullOrWhiteSpace(model.InstallmentDate))
                {
                    order.InstallmentDate = DateTime.Parse(model.InstallmentDate);
                    order.InstallmentDate = order.InstallmentDate.FromPacificTime();
                }


                order.Adjustment = model.Adjustment;
                order.AdjustmentReason = model.AdjustmentReason;
                if(order.Adjustment != 0)
                {
                    order.Total = order.Adjustment + order.SubTotal;
                    order.BalanceRemaining = order.Total;
                }
            }
            // Deal with OrderMeta data
            foreach (var metaData in model.MetaData)
            {
                order.AddMetaData(metaData.Name, metaData.Value);
            }

            //We allow it to be created with this, but must be added and valid when it is submitted
            if (model.Billings.Any())
            {
                var updateBilling = await UpdateOrderBillingInfo(order, model);
                if (!updateBilling.Success)
                {
                    rtValue = updateBilling;
                    return rtValue;
                }
            }

            await _dbContext.Orders.AddAsync(order);

            await _historyService.OrderCreated(order, currentUser);
            await _historyService.OrderSnapshot(order, currentUser, History.OrderActions.Created);

            rtValue.Success = true;
            rtValue.Order = order;
            
            return rtValue;
        }

        private async Task<ProcessingResult> UpdateExistingOrder(OrderPostModel model, bool isClusterOrSystemAdmin, User currentUser)
        {
            var rtValue = new ProcessingResult();

            //Updating an existing order without changing the status.
            var existingOrder = await _dbContext.Orders.Include(a => a.PrincipalInvestigator.Owner).Include(a => a.Cluster).Include(a => a.Billings).Include(a => a.MetaData).FirstAsync(a => a.Id == model.Id);            

            switch (existingOrder.Status)
            {
                case Order.Statuses.Created:
                    if(existingOrder.PrincipalInvestigator.Owner != currentUser)
                    {
                        rtValue.Success = false;
                        rtValue.Message = $"Only the sponsor/PI can edit an order in the {existingOrder.Status} status.";
                        return rtValue;
                    }

                    break;
                case Order.Statuses.Submitted:
                    rtValue.Success = false;
                    rtValue.Message = "Order may not be edited in the Submitted status.";
                    return rtValue;

                case Order.Statuses.Processing:
                    if (!isClusterOrSystemAdmin)
                    {
                        rtValue.Success = false;
                        rtValue.Message = $"Only admins can edit an order in the {existingOrder.Status} status.";
                        return rtValue;
                    }

                    break;

                case Order.Statuses.Active:
                    if (!isClusterOrSystemAdmin)
                    {
                        rtValue.Success = false;
                        rtValue.Message = $"Only admins can edit an order in the {existingOrder.Status} status.";
                        return rtValue;
                    }
                    //Only allow the dates and maybe the admin notes?

                    break;
                default:
                    rtValue.Success = false;
                    rtValue.Message = "This order is in a status that doesn't support editing";
                    return rtValue;

            }

            await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Updated); //Before Changes -- I think this will work?

            if(existingOrder.Status == Order.Statuses.Created || existingOrder.Status == Order.Statuses.Submitted || existingOrder.Status == Order.Statuses.Processing)
            {
                if (isClusterOrSystemAdmin)
                {
                    existingOrder.Category = model.Category;
                    existingOrder.ProductName = model.ProductName;
                    existingOrder.Description = model.Description;
                    existingOrder.Adjustment = model.Adjustment;
                    existingOrder.AdjustmentReason = model.AdjustmentReason;
                    if (existingOrder.Adjustment != 0)
                    {
                        existingOrder.Total = existingOrder.Adjustment + existingOrder.SubTotal;
                        existingOrder.BalanceRemaining = existingOrder.Total;
                    }
                    existingOrder.AdminNotes = model.AdminNotes;
                    existingOrder.InstallmentType = model.InstallmentType; //TODO, validate that this is set correctly
                    existingOrder.Installments = model.Installments;
                    existingOrder.UnitPrice = model.UnitPrice;
                    existingOrder.Units = model.Units;
                    existingOrder.ExternalReference = model.ExternalReference;
                    existingOrder.LifeCycle = model.LifeCycle; //Number of months or years the product is active for
                    if (!string.IsNullOrWhiteSpace(model.ExpirationDate))
                    {
                        existingOrder.ExpirationDate = DateTime.Parse(model.ExpirationDate);
                        existingOrder.ExpirationDate = existingOrder.ExpirationDate.FromPacificTime();
                        //DateTime.TryParse(model.ExpirationDate, out var expirationDate); //I could do a try parse, but if the parse fails it should throw an error?
                        //existingOrder.ExpirationDate = expirationDate;
                    }
                    else
                    {
                        //TODO: Can we allow this to be cleared out?
                        existingOrder.ExpirationDate = null;
                    }

                    if (!string.IsNullOrWhiteSpace(model.InstallmentDate))
                    {
                        existingOrder.InstallmentDate = DateTime.Parse(model.InstallmentDate);
                        existingOrder.InstallmentDate = existingOrder.InstallmentDate.FromPacificTime();
                    }
                    else
                    {
                        existingOrder.InstallmentDate = null;
                    }
                }

                existingOrder.Description = model.Description;
                existingOrder.Name = model.Name;
                existingOrder.Notes = model.Notes;
                //existingOrder.Quantity = model.Quantity; //If quantity changes, we will need to update the total and balance remaining
                if(existingOrder.Quantity != model.Quantity)
                {
                    existingOrder.Quantity = model.Quantity;
                    existingOrder.SubTotal = model.Quantity * existingOrder.UnitPrice;
                    existingOrder.Total = model.Quantity * existingOrder.UnitPrice;
                    existingOrder.BalanceRemaining = model.Quantity * existingOrder.UnitPrice;
                    if(existingOrder.Adjustment != 0)
                    {
                        existingOrder.Total = existingOrder.Adjustment + existingOrder.SubTotal;
                        existingOrder.BalanceRemaining = existingOrder.Total;
                    }
                }

                ProcessMetaData(model, existingOrder);

                var updateBilling = await UpdateOrderBillingInfo(existingOrder, model);
                if (!updateBilling.Success)
                {
                    return updateBilling;
                }
            }

            if (existingOrder.Status == Order.Statuses.Active)
            {
                //We will only allow these to be changed, not cleared out once active (Maybe?...)
                if (!string.IsNullOrWhiteSpace(model.ExpirationDate))
                {
                    existingOrder.ExpirationDate = DateTime.Parse(model.ExpirationDate);
                    existingOrder.ExpirationDate = existingOrder.ExpirationDate.FromPacificTime();
                    //DateTime.TryParse(model.ExpirationDate, out var expirationDate); //I could do a try parse, but if the parse fails it should throw an error?
                    //existingOrder.ExpirationDate = expirationDate;
                }
                else
                {                   
                    rtValue.Success = false;
                    rtValue.Message = "Expiration Date is required for an order in the Active status.";
                    return rtValue;
                }

                if (!string.IsNullOrWhiteSpace(model.InstallmentDate))
                {
                    existingOrder.InstallmentDate = DateTime.Parse(model.InstallmentDate);
                    existingOrder.InstallmentDate = existingOrder.InstallmentDate.FromPacificTime();
                }
                else
                {
                    rtValue.Success = false;
                    rtValue.Message = "Installment Date is required for an order in the Active status.";
                    return rtValue;
                }
                existingOrder.AdminNotes = model.AdminNotes;
            }


            await _historyService.OrderSnapshot(existingOrder, currentUser, History.OrderActions.Updated); //After Changes
            await _historyService.OrderUpdated(existingOrder, currentUser);

            rtValue.Success = true;
            rtValue.Order = existingOrder;

            return rtValue;
        }

        private void ProcessMetaData(OrderPostModel model, Order existingOrder)
        {
            var metaDatasToRemove = new List<OrderMetaData>();
            //Deal with OrderMeta data (Test this)
            foreach (var metaData in existingOrder.MetaData)
            {
                if (model.MetaData.Any(a => a.Id == metaData.Id))
                {
                    //Possibly update values
                    metaData.Value = model.MetaData.First(a => a.Id == metaData.Id).Value;
                    metaData.Name = model.MetaData.First(a => a.Id == metaData.Id).Name;
                }
                else
                {
                    metaDatasToRemove.Add(metaData);
                }
            }
            foreach (var metaData in metaDatasToRemove)
            {
                existingOrder.MetaData.Remove(metaData);
            }

            foreach (var metaData in model.MetaData.Where(a => a.Id == 0)) //New Values -- add them
            {
                existingOrder.AddMetaData(metaData.Name, metaData.Value);
            }
        }

        private async Task<ProcessingResult> UpdateOrderBillingInfo(Order order, OrderPostModel model)
        {
            if (model.Billings.Sum(a => a.Percentage) != 100) //Maybe make this dependent on the status? Created we allow bad data, but submitted we don't.
            {
                return new ProcessingResult { Success = false, Message = "The sum of the percentages must be 100%." };
            }

            //Validate and fix and passed chart strings
            foreach(var mBilling in model.Billings)
            {
                var chartStringValidation = await _aggieEnterpriseService.IsChartStringValid(mBilling.ChartString, Directions.Debit);
                if (chartStringValidation.IsValid == false)
                {
                    return new ProcessingResult { Success = false, Message = $"Invalid Chart String: {chartStringValidation.Message}" };
                }
                mBilling.ChartString = chartStringValidation.ChartString;
            }

            //Check for duplicate chart strings
            var duplicateChartStrings = model.Billings.GroupBy(a => a.ChartString).Where(a => a.Count() > 1).Select(a => a.Key).ToList();
            if (duplicateChartStrings.Any())
            {
                return new ProcessingResult { Success = false, Message = $"Duplicate Chart Strings found: {string.Join(", ", duplicateChartStrings)}" };
            }


            //Make sure there are no duplicate chart strings?
            //Allow Admin side to save invalid billings?
            //Probably passing the ID? 
            var billingsToRemove = new List<Billing>();
            foreach (var billing in order.Billings)
            {
                if (model.Billings.Any(a => a.ChartString == billing.ChartString))
                {
                    billing.Percentage = model.Billings.First(a => a.ChartString == billing.ChartString).Percentage;
                }
                else
                {
                    billingsToRemove.Add(billing);
                }
            }
            foreach (var billing in model.Billings)
            {
                if (order.Billings.Any(a => a.ChartString == billing.ChartString))
                {
                    //Nothing to do, it is already there
                }
                else
                {
                    order.Billings.Add(new Billing
                    {
                        ChartString = billing.ChartString,
                        Percentage = billing.Percentage,
                        Order = order,
                        Updated = DateTime.UtcNow
                    });
                }
            }

            if (billingsToRemove.Any())
            {
                foreach (var billing in billingsToRemove)
                {
                    order.Billings.Remove(billing);
                }
            }



            return new ProcessingResult { Success = true };
        }

        private class ProcessingResult
        {
            public bool Success { get; set; }
            public string? Message { get; set; }

            public Order? Order { get; set; }
        }
    }
}
