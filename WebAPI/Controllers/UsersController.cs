using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecommerce.core.DTOs;
using WebAPI.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IMapper _mapper;

        public UsersController(EcommerceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            var dto = _mapper.Map<UserDTO>(user);
            return Ok(dto);
        }

        // GET api/users/byemail/user@example.com
        [HttpGet("byemail/{email}")]
        public async Task<ActionResult<UserDTO>> GetUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound();

            var dto = _mapper.Map<UserDTO>(user);
            return Ok(dto);
        }

        // PUT api/users/updateprofile/user@example.com
        [HttpPut("updateprofile/{email}")]  
        public async Task<IActionResult> UpdateUserProfile(string email, [FromBody] UserDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound();

            user.Name = dto.Name;
            user.Surname = dto.Surname;
            user.Username = dto.Username;
            user.Email = dto.Email;
            user.Phone = dto.Phone;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch
            {
                return StatusCode(500, "Failed to update user.");
            }
        }

        // Get: api/users/with-orders
        [HttpGet("with-orders")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserWithOrdersDTO>>> GetUsersWithOrders()
        {
            var users = await _context.Users
                .Include(u => u.Orders)
                    .ThenInclude(c => c.Product)
                .ToListAsync();

            var dtos = _mapper.Map<List<UserWithOrdersDTO>>(users);
            return Ok(dtos);
        }

    }
}
