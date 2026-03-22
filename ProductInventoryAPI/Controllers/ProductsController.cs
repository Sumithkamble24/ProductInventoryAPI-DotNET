using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductInventoryAPI.Data;
using ProductInventoryAPI.Models;

namespace ProductInventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // Get all products with optional filter and sort
        [HttpGet]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] string? category = null,
            [FromQuery] string? sortByPrice = null)
        {
            try
            {
                var products = await _context.Products.ToListAsync();

                // filter by category if provided
                if (!string.IsNullOrWhiteSpace(category))
                {
                    products = products.Where(p => p.Category.ToLower() == category.ToLower()).ToList();
                }

                // sort by price
                if (sortByPrice == "asc")
                {
                    products = products.OrderBy(p => p.Price).ToList();
                }
                else if (sortByPrice == "desc")
                {
                    products = products.OrderByDescending(p => p.Price).ToList();
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Get out of stock products
        [HttpGet("out-of-stock")]
        public async Task<IActionResult> GetOutOfStockProducts()
        {
            var outOfStock = await _context.Products
                .Where(p => p.StockQuantity == 0)
                .ToListAsync();

            if (outOfStock == null || outOfStock.Count == 0)
                return NotFound("No out of stock products found.");

            return Ok(outOfStock);
        }

        // Get product by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            if (id <= 0)
                return BadRequest("ID must be a positive number.");

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound($"Product with ID {id} was not found.");

            return Ok(product);
        }

        // Add new product
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product newProduct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
        }

        // Update existing product
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            if (id <= 0)
                return BadRequest("ID must be a positive number.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Products.FindAsync(id);

            if (existing == null)
                return NotFound($"Product with ID {id} not found.");

            // update the fields
            existing.Name = updatedProduct.Name;
            existing.Category = updatedProduct.Category;
            existing.Price = updatedProduct.Price;
            existing.StockQuantity = updatedProduct.StockQuantity;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // Delete product by id
        [HttpDelete("{id}")]
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