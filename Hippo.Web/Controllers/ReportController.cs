using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models.ReportModels;
using Hippo.Core.Services;
using Hippo.Web.Models.OrderModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static Hippo.Core.Models.SlothModels.TransferViewModel;

//TODO: Remove unused usings

namespace Hippo.Web.Controllers
{
    [Authorize]
    public class ReportController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;

        public ReportController(AppDbContext dbContext, IUserService userService, IAggieEnterpriseService aggieEnterpriseService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _aggieEnterpriseService = aggieEnterpriseService;
        }

        [HttpGet]
        public async Task<IActionResult> Payments(string filterType, string start, string end)
        {
            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);
            var isFinancialAdmin = permissions.IsFinancialAdmin(Cluster);

            if (!isClusterOrSystemAdmin && !isFinancialAdmin)
            {
                return BadRequest("You do not have permission to view this page.");
            }

            var query = _dbContext.Payments.Include(a => a.Order.PrincipalInvestigator).Include(a => a.Order.MetaData).Where(a => a.Order.Cluster.Name == Cluster && a.Status == Payment.Statuses.Completed && a.CompletedOn != null);
            var orderQuery = _dbContext.Orders.Where(a => a.Cluster.Name == Cluster); //Will only be used for the filter using order info
            List<int> orderIds = new List<int>();

            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrWhiteSpace(start) && start != "undefined")
            {
                try
                {
                    startDate = DateTime.Parse(start).ToUniversalTime();
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error parsing start date");
                }

            }
            if (!string.IsNullOrWhiteSpace(end) && end != "undefined")
            {
                try
                {
                    endDate = DateTime.Parse(end).ToUniversalTime();
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error parsing end date");
                }

            }

            switch (filterType)
            {
                case "PaymentDate":
                    if (startDate.HasValue)
                    {
                        query = query.Where(a => a.CompletedOn >= startDate.Value);
                    }
                    if (endDate.HasValue)
                    {
                        query = query.Where(a => a.CompletedOn <= endDate.Value);
                    }
                    break;
                case "OrderExpiryDate":
                    if (startDate.HasValue)
                    {
                        orderQuery = orderQuery.Where(a => a.ExpirationDate != null && a.ExpirationDate >= startDate.Value);
                    }
                    if (endDate.HasValue)
                    {
                        orderQuery = orderQuery.Where(a => a.ExpirationDate != null && a.ExpirationDate <= endDate.Value);
                    }
                    orderIds = await orderQuery.Select(a => a.Id).ToListAsync();
                    query = query.Where(a => orderIds.Contains(a.OrderId));
                    break;
                case "OrderInstallmentDate":
                    if (startDate.HasValue)
                    {
                        orderQuery = orderQuery.Where(a => a.InstallmentDate != null && a.InstallmentDate >= startDate.Value);
                    }
                    if (endDate.HasValue)
                    {
                        orderQuery = orderQuery.Where(a => a.InstallmentDate != null && a.InstallmentDate <= endDate.Value);
                    }
                    orderIds = await orderQuery.Select(a => a.Id).ToListAsync();
                    query = query.Where(a => orderIds.Contains(a.OrderId));
                    break;
                case "OrderCreationDate":
                    if (startDate.HasValue)
                    {
                        orderQuery = orderQuery.Where(a => a.CreatedOn >= startDate.Value);
                    }
                    if (endDate.HasValue)
                    {
                        orderQuery = orderQuery.Where(a => a.CreatedOn <= endDate.Value);
                    }
                    orderIds = await orderQuery.Select(a => a.Id).ToListAsync();
                    query = query.Where(a => orderIds.Contains(a.OrderId));
                    break;
                default:
                    return BadRequest("Invalid filter type.");
            }


            var model = await query.Select(InvoiceModel.Projection()).ToListAsync();

            return Ok(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExpiringOrders()
        {
            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            if (!isClusterOrSystemAdmin)
            {
                return BadRequest("You do not have permission to view this page.");
            }

            //TODO: Need to filter out recurring?

            var statuses = new string[] { Order.Statuses.Active, Order.Statuses.Completed};
            var compareDate = DateTime.UtcNow.AddDays(31);
            var orders = await _dbContext.Orders.Include(a => a.PrincipalInvestigator).Include(a => a.MetaData).Where(a => a.Cluster.Name == Cluster && statuses.Contains(a.Status) && a.ExpirationDate != null && a.ExpirationDate <= compareDate).Select(OrderListModel.Projection()).ToListAsync();

            return Ok(orders);
        }

        [HttpGet]
        public async Task<IActionResult> ArchivedOrders()
        {
            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            if (!isClusterOrSystemAdmin)
            {
                return BadRequest("You do not have permission to view this page.");
            }
            var orders = await _dbContext.Orders.Include(a => a.PrincipalInvestigator).Include(a => a.MetaData).Where(a => a.Cluster.Name == Cluster && a.Status == Order.Statuses.Archived).Select(OrderListModel.Projection()).ToListAsync();

            return Ok(orders);
        }

        [HttpGet]
        public async Task<IActionResult> ProblemOrders()
        {
            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            if (!isClusterOrSystemAdmin)
            {
                return BadRequest("You do not have permission to view this page.");
            }
            var orders = await _dbContext.Orders.Include(a => a.Billings).Where(a => a.Cluster.Name == Cluster && a.Status == Order.Statuses.Active).ToListAsync();

            var uniqueChartStrings = orders.SelectMany(a => a.Billings.Where(a => a.ChartString != null).Select(b => b.ChartString)).Distinct().ToList();

            //Check for and validate these order's chart strings
            var invalidChartStringOrWarnings = new List<Dictionary<string, string>>();

            foreach (var chartString in uniqueChartStrings)
            {

                if (chartString == null)
                {
                    continue;
                }
                var chartStringValidation = await _aggieEnterpriseService.IsChartStringValid(chartString, Directions.Debit);
                if (!chartStringValidation.IsValid)
                {
                    var invalidEntry = new Dictionary<string, string> { { chartString, chartStringValidation.Message } };
                    invalidChartStringOrWarnings.Add(invalidEntry);
                }
                else
                {
                    if (!string.IsNullOrEmpty(chartStringValidation.Warning))
                    {
                        var invalidEntry = new Dictionary<string, string> { { chartString, chartStringValidation.Warning } };
                        invalidChartStringOrWarnings.Add(invalidEntry);
                    }
                }

            }


            //Find all orders that have a billing with a chart string that is in the invalidChartStringOrWarnings
            var problemOrders = orders.Where(a => a.Billings.Any(b => invalidChartStringOrWarnings.Any(c => c.ContainsKey(b.ChartString)))).ToList();


            var billingIssues = orders.AsQueryable().Where(a => a.NextPaymentDate <= DateTime.UtcNow.AddDays(5)).ToList();

            //get a list of order ids and combile them from both problemOrders and billingIssues
            var orderIds = problemOrders.Select(a => a.Id).Union(billingIssues.Select(a => a.Id)).ToList();

            var model = await _dbContext.Orders.Include(a => a.PrincipalInvestigator).Include(a => a.MetaData).Where(a => orderIds.Contains(a.Id)).Select(OrderListModel.Projection()).ToListAsync();

            if(problemOrders.Count > 0)
            {
                foreach (var order in problemOrders)
                {
                    var messages = new List<string>();
                    messages.Add("Chart String Problem:");
                    foreach (var billing in order.Billings)
                    {
                        //check if the chart string is in the invalidChartStringOrWarnings
                        if (invalidChartStringOrWarnings.Any(a => a.ContainsKey(billing.ChartString)))
                        {
                            messages.Add(invalidChartStringOrWarnings.First(a => a.ContainsKey(billing.ChartString))[billing.ChartString]);
                        }
                    }
                    //get the order from the model and add the messages
                    model.First(a => a.Id == order.Id).Messages = string.Join(" ", messages);
                }
            }
            if(billingIssues.Count > 0)
            {
                foreach (var order in billingIssues)
                {
                    //get the order from the model and add the messages
                    model.First(a => a.Id == order.Id).Messages = $"Stale Next Payment Date. {model.First(a => a.Id == order.Id).Messages}";
                }
            }

            return Ok(model);
        }
    }
}
