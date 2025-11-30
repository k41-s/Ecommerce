using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Ecommerce.core.DTOs;
using WebAPI.Models;
using AutoMapper;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IMapper _mapper;

        public OrderController(EcommerceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // POST: api/order/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == dto.ProductId);

                if (product == null)
                    return NotFound("Product not found");

                var order = _mapper.Map<CustomerOrder>(dto);

                _context.CustomerOrders.Add(order);
                await _context.SaveChangesAsync();

                return Ok("Order requested.");
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        // GET: api/order/user/5
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetUserOrders(int userId)
        {
            var orders = await _context.CustomerOrders
                .Include(c => c.Product)
                    .ThenInclude(p => p.ProductImages)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.OrderedAt)
                .ToListAsync();

            if (!orders.Any())
                return NotFound();

            var orderDTOs = _mapper.Map<List<OrderDTO>>(orders);

            return Ok(orderDTOs);
        }

        // GET: api/order/admin
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAllOrder()
        {
            var CustomerOrder = await _context.CustomerOrders
                .Include(c => c.Product)
                .Include(c => c.User)
                .OrderByDescending(c => c.OrderedAt)
                .Select(c => new OrderDTO
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ProductName = $"{c.Product.Name}",
                    UserId = c.UserId,
                    UserName = $"{c.User.Name} {c.User.Surname}",
                    OrderedAt = c.OrderedAt,
                    PaymentMethod = c.PaymentMethod,
                    Notes = c.Notes
                })
                .ToListAsync();

            return Ok(CustomerOrder);
        }
    }
}
