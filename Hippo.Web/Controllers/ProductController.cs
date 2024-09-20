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
    public class ProductController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private IUserService _userService;

        public ProductController(AppDbContext dbContext, IUserService userService)
        {
            _dbContext = dbContext;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userService.GetCurrentUser();
            var permissions = await _userService.GetCurrentPermissionsAsync();
            var isClusterOrSystemAdmin = permissions.IsClusterOrSystemAdmin(Cluster);
            var isFinancialAdmin = permissions.IsFinancialAdmin(Cluster);

            List<Product> products = null;
            if (!isClusterOrSystemAdmin && !isFinancialAdmin)
            {
                products = await _dbContext.Products.Where(a => a.Cluster.Name == Cluster && !a.IsHiddenFromPublic).ToListAsync();
            }
            else
            {
                products = await _dbContext.Products.Where(a => a.Cluster.Name == Cluster).ToListAsync(); //Filters out inactive products
            }
            
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.ClusterAdminAccess)]
        public async Task<IActionResult> CreateProduct([FromBody] Product model)
        {
            var cluster = await _dbContext.Clusters.FirstAsync(a => a.Name == Cluster);

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid");
            }
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return BadRequest("Name is required");
            }
            if (string.IsNullOrWhiteSpace(model.Description))
            {
                return BadRequest("Description is required");
            }
            if (string.IsNullOrWhiteSpace(model.Category))
            {
                return BadRequest("Category is required");
            }
            if (model.UnitPrice <= 0)
            {
                return BadRequest("Unit Price must be greater than 0");
            }
            if (string.IsNullOrWhiteSpace(model.Units))
            {
                return BadRequest("Units is required");
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Category = model.Category,
                UnitPrice = model.UnitPrice,
                Units = model.Units,
                Cluster = cluster,
                Installments = model.Installments,
                InstallmentType = model.InstallmentType,
                LifeCycle = model.LifeCycle,
                IsRecurring = model.IsRecurring,
                IsUnavailable = model.IsUnavailable,
                IsHiddenFromPublic = model.IsHiddenFromPublic,
            };
            if(product.InstallmentType == Product.InstallmentTypes.OneTime)
            {
                product.Installments = 1;
            }
            if(product.IsRecurring && product.InstallmentType == Product.InstallmentTypes.OneTime)
            {
                return BadRequest("Recurring products must have a recurring installment type other than One Time");
            }
            if (product.IsRecurring)
            {
                product.Installments = 0;
                product.LifeCycle = 0; //Maybe we want a lifecycle, but I don't know how that would work with recurring products
            }

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            return Ok(product);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.ClusterAdminAccess)]
        public async Task<IActionResult> UpdateProduct([FromBody] Product model)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(a => a.Id == model.Id && a.Cluster.Name == Cluster);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Category = model.Category;
            product.UnitPrice = model.UnitPrice;
            product.Units = model.Units;
            product.Installments = model.Installments;
            product.InstallmentType = model.InstallmentType;
            product.LifeCycle = model.LifeCycle;
            product.IsUnavailable = model.IsUnavailable;
            product.IsHiddenFromPublic = model.IsHiddenFromPublic;
            product.IsRecurring = model.IsRecurring;
            if (product.InstallmentType == Product.InstallmentTypes.OneTime)
            {
                product.Installments = 1;
            }
            if (product.IsRecurring && product.InstallmentType == Product.InstallmentTypes.OneTime)
            {
                return BadRequest("Recurring products must have a recurring installment type other than One Time");
            }
            if (product.IsRecurring)
            {
                product.Installments = 0;
                product.LifeCycle = 0; //Maybe we want a lifecycle, but I don't know how that would work with recurring products
            }

            await _dbContext.SaveChangesAsync();

            return Ok(product);
        }   

        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(a => a.Id == id && a.Cluster.Name == Cluster);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.ClusterAdminAccess)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(a => a.Id == id && a.Cluster.Name == Cluster);
            if (product == null)
            {
                return NotFound();
            }

            product.IsActive = false;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
