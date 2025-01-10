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
using Serilog;
using Hippo.Core.Models.Email;
using Hippo.Core.Models.ReportModels;

//TODO: Remove unused usings

namespace Hippo.Web.Controllers
{
    [Authorize]
    public class ReportController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public ReportController(AppDbContext dbContext, IUserService userService)
        {
            _dbContext = dbContext;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Payments(string filterType, string start, string end)
        {
            //TODO: use the parameters

            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);
            var isFinancialAdmin = permissions.IsFinancialAdmin(Cluster);

            if (!isClusterOrSystemAdmin && !isFinancialAdmin)
            {
                return BadRequest("You do not have permission to view this page.");
            }

            var query = _dbContext.Payments.Include(a => a.Order.PrincipalInvestigator).Include(a => a.Order.MetaData).Where(a => a.Order.Cluster.Name == Cluster && a.Status == Payment.Statuses.Completed && a.CompletedOn != null);
            if(start != null && start != "undefined")
            {
                try
                {
                    var date = DateTime.Parse(start).ToUniversalTime();
                    query = query.Where(a => a.CompletedOn >= date);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error parsing start date");
                }
                
            }
            if (end != null && end != "undefined")
            {
                try
                {
                    var date = DateTime.Parse(end).ToUniversalTime();
                    query = query.Where(a => a.CompletedOn <= date);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error parsing end date");
                }

            }

            //var payments = await _dbContext.Payments.Include(a => a.Order.PrincipalInvestigator).Include(a => a.Order.MetaData).Where(a => a.Order.Cluster.Name == Cluster && a.Status == Payment.Statuses.Completed && ids.Contains(a.OrderId)).Select(InvoiceModel.Projection()).ToListAsync();
            //var payments2 = await _dbContext.Payments.Include(a => a.Order.PrincipalInvestigator).Include(a => a.Order.MetaData).Where(a => a.Status == Payment.Statuses.Completed).Select(InvoiceModel.Projection()).ToListAsync();

            //var model = await _dbContext.Payments.Include(a => a.Order.PrincipalInvestigator).Include(a => a.Order.MetaData).Where(a => a.Order.Cluster.Name == Cluster && a.Status == Payment.Statuses.Completed).Select(InvoiceModel.Projection()).ToListAsync();

            var model = await query.Select(InvoiceModel.Projection()).ToListAsync();

            return Ok(model);
        }
    }
}
