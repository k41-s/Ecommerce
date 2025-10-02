using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ecommerce.core.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(EcommerceContext context, IConfiguration config, IMapper mapper)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
        }

        // Helper method to hash a password string
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists.");

            // Use AutoMapper to map RegisterUserDTO to User
            var user = _mapper.Map<User>(dto);

            // Hash password manually (since mapping ignores it)
            user.PasswordHash = HashPassword(dto.Password);
            user.Role = "User"; // assign default role

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User registered successfully.");
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticatedUserDTO>> Login([FromBody] LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || user.PasswordHash != HashPassword(dto.Password))
                return Unauthorized("Invalid Username or password.");

            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_config["Jwt:ExpireHours"])),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Use AutoMapper to map User to AuthenticatedUserDTO
            var authUserDto = _mapper.Map<AuthenticatedUserDTO>(user);
            authUserDto.Token = tokenHandler.WriteToken(token);


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, authUserDto.Username),
                new Claim(ClaimTypes.Role, authUserDto.Role),
                new Claim("AccessToken", authUserDto.Token)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Ok(authUserDto);
        }

        // POST: api/auth/changepassword
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || user.PasswordHash != HashPassword(dto.OldPassword))
                return Unauthorized("Invalid credentials.");

            user.PasswordHash = HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return Ok("Password changed successfully.");
        }
    }
}
