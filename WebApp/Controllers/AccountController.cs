using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.ViewModels;
using AutoMapper;
using System.Net.Http.Json;
using Ecommerce.core.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMapper _mapper;

        public AccountController(IHttpClientFactory clientFactory, IMapper mapper)
        {
            _clientFactory = clientFactory;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized();

            var client = _clientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"api/users/byemail/{userEmail}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var userDto = await response.Content.ReadFromJsonAsync<UserDTO>();
            var vm = _mapper.Map<ProfileViewModel>(userDto);

            ViewBag.IsAdmin = User.IsInRole("Admin");

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = _clientFactory.CreateClient("ApiClient");
            var userDto = _mapper.Map<UserDTO>(vm);

            var response = await client.PutAsJsonAsync($"api/users/updateprofile/{userDto.Email}", userDto);

            if (response.IsSuccessStatusCode)
                return Json(new { success = true });

            return Json(new { success = false, message = "Could not update profile." });
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null, string? reason = null)
        {
            if (reason == "unauthorized")
                TempData["Message"] = "You must be logged in to access that page";

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginViewModel model)
        {
            if (!ModelState.IsValid) 
                return View(model);

            var client = _clientFactory.CreateClient("ApiClient");
            var loginDto = _mapper.Map<LoginDTO>(model);

            var response = await client.PostAsJsonAsync("api/auth/login", loginDto);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }

            var authenticatedUser = await response.Content.ReadFromJsonAsync<AuthenticatedUserDTO>();

            if (authenticatedUser == null)
            {
                ModelState.AddModelError("", "Login failed. Please try again.");
                return RedirectToAction("Login");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, authenticatedUser.Username),
                new Claim(ClaimTypes.Email, authenticatedUser.Email),
                new Claim(ClaimTypes.Role, authenticatedUser.Role),
                new Claim("AccessToken", authenticatedUser.Token)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (authenticatedUser.Role == "Admin")
                return RedirectToAction("Index", "Product");
            else
                return RedirectToAction("Index", "UserView");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (vm.Password != vm.RepeatPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(vm);
            }

            var client = _clientFactory.CreateClient("ApiClient");
            var registerDto = _mapper.Map<RegisterUserDTO>(vm);

            var response = await client.PostAsJsonAsync("api/auth/register", registerDto);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Email already in use or registration failed.");
                return View(vm);
            }

            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}
