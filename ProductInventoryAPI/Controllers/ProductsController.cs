using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductInventoryAPI.Data;
using ProductInventoryAPI.Models;
using System.Security.Claims;


namespace ProductInventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Viewer")]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] string? category = null,
            [FromQuery] string? sortByPrice = null)
        {
            try
            {
                var query = _context.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(category))
                {
                    query = query.Where(p => p.Category.ToLower() == category.ToLower());
                }

                if (sortByPrice == "asc")
                {
                    query = query.OrderBy(p => p.Price);
                }
                else if (sortByPrice == "desc")
                {
                    query = query.OrderByDescending(p => p.Price);
                }

                var products = await query.ToListAsync();

                var calledBy = User.FindFirst(ClaimTypes.Name)?.Value;
                var callerRole = User.FindFirst(ClaimTypes.Role)?.Value;

                return Ok(new
                {
                    calledBy,
                    callerRole,
                    data = products
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("out-of-stock")]
        [Authorize(Roles = "Admin,Manager,Viewer")]
        public async Task<IActionResult> GetOutOfStockProducts()
        {
            var outOfStock = await _context.Products
                .Where(p => p.StockQuantity == 0)
                .ToListAsync();

            if (!outOfStock.Any())
                return NotFound("No out of stock products found.");

            return Ok(outOfStock);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Viewer")]
        public async Task<IActionResult> GetProductById(int id)
        {
            if (id <= 0)
                return BadRequest("ID must be a positive number.");

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound($"Product with ID {id} was not found.");

            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddProduct([FromBody] Product newProduct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            if (id <= 0)
                return BadRequest("ID must be a positive number.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Products.FindAsync(id);

            if (existing == null)
                return NotFound($"Product with ID {id} not found.");

            existing.Name = updatedProduct.Name;
            existing.Category = updatedProduct.Category;
            existing.Price = updatedProduct.Price;
            existing.StockQuantity = updatedProduct.StockQuantity;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (id <= 0)
                return BadRequest("ID must be a positive number.");

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound($"Product with ID {id} not found.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok($"Product '{product.Name}' (ID: {id}) has been deleted.");
        }
    }
}