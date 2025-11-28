using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IMapper _mapper;

        public ProductController(EcommerceContext context, IMapper mapper)
        {
            _context = context;
            this._mapper = mapper;
        }

        private async Task AddLogAsync(string level, string message)
        {
            Log? logEntry = new Log
            {
                Level = level,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            _context.Logs.Add(logEntry);
            await _context.SaveChangesAsync();
        }

        // Get api/product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            // ProjectTo() ensures EF Core SELECTS only the Image IDs and explicitly 
            // IGNORES the heavy 'Data' (byte[]) column from the database.
            var productDTOs = await _context.Products
                .AsNoTracking()
                .ProjectTo<ProductDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(productDTOs);
        }

        // Get api/product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            try
            {
                var productDTO = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.Id == id)
                    .ProjectTo<ProductDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (productDTO == null)
                {
                    await AddLogAsync("Warning", $"Product with id={id} not found");
                    return NotFound();
                }

                await AddLogAsync("Information", $"Product with id={id} retrieved");

                return Ok(productDTO);
            }
            catch (Exception ex)
            {
                await AddLogAsync("Error", $"Error retrieving product: {ex.Message}");
                return StatusCode(500);
            }
        }

        // Post api/product
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> Create([FromBody] ProductDTO dto)
        {
            try
            {
                Category? category = await _context.Categories
                    .FindAsync(dto.CategoryId);

                if (category == null)
                    return BadRequest("Invalid Category Id");

                List<Country>? countries = await _context.Countries
                    .Where(o => dto.CountryIds.Contains(o.Id))
                    .ToListAsync();

                Product product = _mapper.Map<Product>(dto);
                product.Category = category;
                product.Countries = countries;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                await AddLogAsync("Information", $"Product with Id={product.Id} created");

                var productDTO = _mapper.Map<ProductDTO>(product);

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDTO);
            }
            catch (Exception ex)
            {
                await AddLogAsync("Error", $"Error creating product:{ex.Message}");
                return StatusCode(500);
            }
        }

        // Put api/product/5
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id, [FromBody] ProductDTO dto)
        {
            if (id <= 0)
                return BadRequest("Invalid product id");
            
            // Note: We do NOT Include ProductImages here becuase theyre updated in their own controller
            Product? product = await _context.Products
                .Include(p => p.Countries)
                .Include(p=> p.Category)
                .FirstOrDefaultAsync(p=> p.Id == id);

            if (product == null)
            {
                await AddLogAsync("Warning", $"Product: id={id} not found during update");
                return NotFound();
            }

            if (id != product.Id)
            {
                await AddLogAsync("Warning", "Update failed: product Id mismatch");
                return BadRequest();
            }

            _mapper.Map(dto, product);

            if (product.CategoryId != dto.CategoryId)
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId);
                if (category == null) return BadRequest("Invalid category Id");
                product.Category = category;
            }

            product.Countries.Clear();

            List<Country> countries = await _context.Countries
                .Where(o => dto.CountryIds.Contains(o.Id))
                .ToListAsync();
            product.Countries = countries;

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await AddLogAsync("Information", $"Product with id={id} updated");
                return NoContent();
            }
            catch(DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id))
                {
                    await AddLogAsync("Warning", $"Product with id={id} not found during update.");
                    return NotFound();
                }
                else
                {
                    await AddLogAsync("Error", $"Concurrency error while updating product id={id}.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                await AddLogAsync("Error", $"Error updating product id={id}: {ex.Message}");
                return StatusCode(500, "Update failed.");
            }
        }
        // DELETE api/product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Countries)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                await AddLogAsync("Warning", $"Product with id={id} not found for deletion.");
                return NotFound();
            }

            try
            {
                // remove foreign keys first
                product.Countries.Clear();

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                await AddLogAsync("Information", $"Product with id={id} deleted.");
                return NoContent();
            }
            catch (Exception ex)
            {
                await AddLogAsync("Error", $"Error deleting product");
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        // GET: api/product/search?query=John&page=1&count=10
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> SearchProducts(
            string? name = null,
            int? categoryId = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                if (_context.Products == null)
                    return NotFound("Product dataset not found.");

                var productsQuery = _context.Products
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    productsQuery = productsQuery.Where(p =>
                        (p.Name != null && p.Name.ToLower().Contains(name.ToLower()))
                    );
                }

                if (categoryId.HasValue && categoryId > 0)
                {
                    productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
                }

                var total = await productsQuery.CountAsync();

                var productDTOs = await productsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ProjectTo<ProductDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                // Log successful search
                await AddLogAsync("Information", $"Product search performed with name filter '{name}', page {page}, count {pageSize}.");

                // Return paged result, optionally include total count in response headers or body
                Response.Headers.Append("X-Total-Count", total.ToString());

                return Ok(productDTOs);
            }
            catch (Exception ex)
            {
                // Log error
                await AddLogAsync("Error", $"Error during product search: {ex.Message}");
                return StatusCode(500, "An error occurred while searching products.");
            }
        }
    }
}
