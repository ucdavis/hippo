using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Extensions;
using Hippo.Core.Models;
using Hippo.Core.Models.ReportModels;
using Hippo.Core.Services;
using Hippo.Web.Models;
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

            var statuses = new string[] { Order.Statuses.Active, Order.Statuses.Completed };
            var compareDate = DateTime.UtcNow.AddDays(31);
            var orders = await _dbContext.Orders
                .Include(a => a.PrincipalInvestigator)
                .Include(a => a.MetaData)
                // We don't want to filter on inactive PIs, we still do on inactive clusters
                .IgnoreQueryFilters().Where(o => o.Cluster.IsActive)
                .Where(a => a.Cluster.Name == Cluster && statuses.Contains(a.Status) && a.ExpirationDate != null && a.ExpirationDate <= compareDate)
                .Select(OrderListModel.Projection())
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet]
        public async Task<IActionResult> AccountDeactivations()
        {
            if (string.IsNullOrWhiteSpace(Cluster))
            {
                return BadRequest("Cluster is required");
            }
            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);

            if (!isClusterOrSystemAdmin)
            {
                return BadRequest("You do not have permission to view this page.");
            }

            // Some of the filtering is more convoluted because Account.MemberOfGroups skips the GroupMemberAccount model
            var accounts = await _dbContext.Accounts
                .IgnoreQueryFilters()
                .Where(a => a.Cluster.Name == Cluster && a.DeactivatedOn != null || _dbContext.GroupMemberAccount
                    .IgnoreQueryFilters()
                    .Any(g => g.AccountId == a.Id && g.RevokedOn != null))
                .Select(a => new
                {
                    Account = a,
                    MemberOfGroups = _dbContext.GroupMemberAccount
                        .IgnoreQueryFilters()
                        .Where(g => g.AccountId == a.Id)
                        .Select(g => new
                        {
                            Group = g.Group,
                            g.RevokedOn
                        })
                        .ToList()
                })
                .ToListAsync();

            var accountModels = accounts.Select(a => new AccountModel
            {
                Id = a.Account.Id,
                Name = a.Account.Name,
                Email = a.Account.Email,
                Kerberos = a.Account.Kerberos,
                CreatedOn = a.Account.CreatedOn,
                Cluster = a.Account.Cluster.Name,
                Owner = a.Account.Owner,
                AcceptableUsePolicyAgreedOn = a.Account.AcceptableUsePolicyAgreedOn,
                MemberOfGroups = a.MemberOfGroups.Select(g => new GroupModel
                {
                    Id = g.Group.Id,
                    DisplayName = g.Group.DisplayName,
                    Name = g.Group.Name,
                    Admins = g.Group.AdminAccounts
                        .Select(admin => new GroupAccountModel
                        {
                            Kerberos = admin.Kerberos,
                            Name = admin.Name,
                            Email = admin.Email
                        }).ToList(),
                    Data = g.Group.Data,
                    RevokedOn = g.RevokedOn
                }).ToList(),
                UpdatedOn = a.Account.UpdatedOn,
                AccessTypes = a.Account.AccessTypes.Select(at => at.Name).ToList(),
                Data = a.Account.Data,
                Tags = a.Account.Tags.Select(t => t.Name).ToList(),
                DeactivatedOn = a.Account.DeactivatedOn,
            })
            .OrderByDescending(x => x.DeactivatedOn)
            .ThenByDescending(x => x.MemberOfGroups.FirstOrDefault(y => y.RevokedOn != null)?.RevokedOn)
            .ToList();

            return Ok(accountModels);
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
            var orders = await _dbContext.Orders
                .Include(a => a.PrincipalInvestigator)
                .Include(a => a.MetaData)
                // We don't want to filter on inactive PIs, we still do on inactive clusters
                .IgnoreQueryFilters().Where(o => o.Cluster.IsActive)
                .Where(a => a.Cluster.Name == Cluster && a.Status == Order.Statuses.Archived)
                .Select(OrderListModel.Projection()).ToListAsync();

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

            var orders = await _dbContext.Orders
                .Include(a => a.Billings)
                .Where(a => a.Cluster.Name == Cluster && a.Status == Order.Statuses.Active)
                .ToListAsync();

            var invalidChartStringOrWarnings = await ValidateChartStrings(orders.Where(a => a.NextPaymentDate != null && a.NextPaymentDate.Value <= DateTime.UtcNow.AddMonths(2)).ToList());

            var problemOrders = orders
                .Where(a => a.NextPaymentDate != null && a.NextPaymentDate.Value <= DateTime.UtcNow.AddMonths(2) && a.Billings.Any(b => invalidChartStringOrWarnings.Any(c => c.ContainsKey(b.ChartString))))
                .ToList();

            var billingIssues = orders
                .Where(a => a.NextPaymentDate <= DateTime.UtcNow.AddDays(-5))
                .ToList();

            var orderIds = problemOrders.Select(a => a.Id).Union(billingIssues.Select(a => a.Id)).ToList();

            var model = await _dbContext.Orders
                .Include(a => a.PrincipalInvestigator)
                .Include(a => a.MetaData)
                // We don't want to filter on inactive PIs, we still do on inactive clusters
                .IgnoreQueryFilters().Where(o => o.Cluster.IsActive)
                .Where(a => orderIds.Contains(a.Id))
                .Select(OrderListModel.Projection())
                .ToListAsync();

            AddMessagesToModel(problemOrders, invalidChartStringOrWarnings, model);
            AddBillingIssueMessages(billingIssues, model);

            return Ok(model);
        }

        private async Task<List<Dictionary<string, string>>> ValidateChartStrings(List<Order> orders)
        {
            var uniqueChartStrings = orders
                .SelectMany(a => a.Billings.Where(a => a.ChartString != null).Select(b => b.ChartString))
                .Distinct()
                .ToList();

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

            return invalidChartStringOrWarnings;
        }

        private void AddMessagesToModel(List<Order> problemOrders, List<Dictionary<string, string>> invalidChartStringOrWarnings, List<OrderListModel> model)
        {
            foreach (var order in problemOrders)
            {
                var messages = new List<string> { "Chart String Problem:" };
                foreach (var billing in order.Billings)
                {
                    if (invalidChartStringOrWarnings.Any(a => a.ContainsKey(billing.ChartString)))
                    {
                        messages.Add(invalidChartStringOrWarnings.First(a => a.ContainsKey(billing.ChartString))[billing.ChartString]);
                    }
                }
                model.First(a => a.Id == order.Id).Messages = string.Join(" ", messages);
            }
        }

        private void AddBillingIssueMessages(List<Order> billingIssues, List<OrderListModel> model)
        {
            foreach (var order in billingIssues)
            {
                model.First(a => a.Id == order.Id).Messages = $"Stale Next Payment Date. {model.First(a => a.Id == order.Id).Messages}";
            }
        }

    }
}
