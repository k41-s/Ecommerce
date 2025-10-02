using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecommerce.core.DTOs;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IMapper _mapper;

        public CategoryController(EcommerceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/category
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAll()
        {
            var types = await _context.Categories.ToListAsync();
            var dtos = _mapper.Map<List<CategoryDTO>>(types);
            return Ok(dtos);
        }

        // GET api/category/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> Get(int id)
        {
            try
            {
                var type = await _context.Categories.FindAsync(id);

                if (type == null)
                    return NotFound();

                var dto = _mapper.Map<CategoryDTO>(type);

                return Ok(dto);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred while retrieving the category.");
            }
        }

        // POST api/category
        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> Post([FromBody] CategoryDTO dto)
        {
            try
            {
                var type = _mapper.Map<Category>(dto);

                _context.Categories.Add(type);
                await _context.SaveChangesAsync();

                var resultDto = _mapper.Map<CategoryDTO>(type);

                return CreatedAtAction(nameof(Get), new { id = type.Id }, resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Creation failed: {ex.Message}");
            }
        }

        // PUT api/category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CategoryDTO dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid id provided");

                var type = await _context.Categories.FindAsync(id);

                if (type == null)
                    return NotFound();

                _mapper.Map(dto, type);

                _context.Entry(type).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(e => e.Id == id))
                    return NotFound();
                else throw;
            }
            catch (Exception)
            {
                return StatusCode(500, "Update failed.");
            }
        }

        // DELETE api/category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var type = await _context.Categories
                    .Include(t => t.Products)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (type == null)
                    return NotFound();

                /* 
                 * Do not want to cascade delete all Products related
                
                type.Products.Clear();
                await _context.SaveChangesAsync();
                */

                _context.Categories.Remove(type);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict("Cannot delete this Category due to related Products.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
