using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Web.Controllers
{
    [Authorize]
    public class ProductController : SuperController
    {
        private readonly AppDbContext _dbContext;

        public ProductController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _dbContext.Products.Where(a => a.Cluster.Name == Cluster).ToListAsync(); //Filters out inactive products
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

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Category = model.Category,
                UnitPrice = model.UnitPrice,
                Units = model.Units,
                Cluster = cluster,
                Installments = model.Installments
            };
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
