using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecommerce.core.DTOs;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/countries")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IMapper _mapper;

        public CountryController(EcommerceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/countries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryDTO>>> GetAll()
        {
            var Countries = await _context.Countries.ToListAsync();
            var CountryDtos = _mapper.Map<List<CountryDTO>>(Countries);
            return Ok(CountryDtos);
        }

        // GET api/countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDTO>> Get(int id)
        {
            try
            {
                var Country = await _context.Countries.FindAsync(id);
                if (Country == null)
                    return NotFound();

                var CountryDto = _mapper.Map<CountryDTO>(Country);
                return Ok(CountryDto);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred while retrieving the Country.");
            }
        }

        // POST api/countries
        [HttpPost]
        public async Task<ActionResult<CountryDTO>> Post([FromBody] CountryDTO CountryCreateDto)
        {
            try
            {
                var Country = _mapper.Map<Country>(CountryCreateDto);
                _context.Countries.Add(Country);
                await _context.SaveChangesAsync();

                var resultDto = _mapper.Map<CountryDTO>(Country);
                return CreatedAtAction(nameof(Get), new { id = Country.Id }, resultDto);
            }
            catch (Exception)
            {
                return StatusCode(500, "Creation failed.");
            }
        }

        // PUT api/countries/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CountryDTO CountryDto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid id provided");

                var Country = await _context.Countries.FindAsync(id);
                if (Country == null)
                    return NotFound();

                // Map changes from DTO to entity
                _mapper.Map(CountryDto, Country);

                _context.Entry(Country).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Countries.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception)
            {
                return StatusCode(500, "Update failed.");
            }
        }

        // DELETE api/countries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var Country = await _context.Countries
                    .Include(a => a.Products)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (Country == null)
                    return NotFound($"Country with ID {id} not found.");

                Country.Products.Clear();
                await _context.SaveChangesAsync();

                _context.Countries.Remove(Country);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict("Cannot delete this Country due to related Products.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
