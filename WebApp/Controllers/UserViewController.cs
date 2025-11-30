using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ecommerce.core.DTOs;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize]
    public class UserViewController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMapper _mapper;

        public UserViewController(IHttpClientFactory clientFactory, IMapper mapper)
        {
            _clientFactory = clientFactory;
            _mapper = mapper;
        }

        // GET: UserView
        public async Task<IActionResult> Index(
            string? name, 
            int? categoryId, 
            int page = 1)
        {
            int pageSize = 10;

            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(name))
                queryParams.Add($"name={Uri.EscapeDataString(name)}");

            if (categoryId.HasValue && categoryId > 0)
                queryParams.Add($"categoryId={categoryId}");

            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            string queryString = string.Join('&', queryParams);

            var response = await client.GetAsync($"/api/product/search?{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                return View("Error", new ErrorViewModel { RequestId = "Failed to load products." });
            }

            var productDtos = await response.Content.ReadFromJsonAsync<List<ProductDTO>>();
            if (productDtos == null)
            {
                return View("Error", new ErrorViewModel { RequestId = "Invalid data from API." });
            }

            var products = _mapper.Map<List<ProductVM>>(productDtos);

            // Read total count from headers for pagination
            int totalProducts = 0;
            if (response.Headers.TryGetValues("X-Total-Count", out var totalValues))
            {
                int.TryParse(totalValues.FirstOrDefault(), out totalProducts);
            }

            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            ViewData["CurrentSearch"] = name;
            ViewData["CurrentCategoryId"] = categoryId;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            var categoryResponse = await client.GetAsync("api/category");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryDtos = await categoryResponse.Content.ReadFromJsonAsync<List<CategoryDTO>>();

                var categorys = _mapper.Map<List<CategoryVM>>(categoryDtos);

                ViewData["CategoryList"] = new SelectList(categorys ?? new List<CategoryVM>(), "Id", "Name");
            }
            else
            {
                ViewData["CategoryList"] = new SelectList(new List<CategoryVM>(), "Id", "Name");
            }

            return View(products);
        }


        // GET: UserView/ProductDetails/5
        public async Task<IActionResult> ProductDetails(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login", "Account");

            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            // Get user by email to find userId
            var userResponse = await client.GetAsync($"api/users/byemail/{email}");
            if (!userResponse.IsSuccessStatusCode)
                return Unauthorized();

            var user = await userResponse.Content.ReadFromJsonAsync<UserDTO>();
            if (user == null)
                return Unauthorized();

            int userId = user.Id;

            // Get product details by id
            var productResponse = await client.GetAsync($"api/product/{id}");
            if (!productResponse.IsSuccessStatusCode)
                return NotFound();

            var productDto = await productResponse.Content.ReadFromJsonAsync<ProductDTO>();
            if (productDto == null)
                return NotFound();

            var product = _mapper.Map<ProductVM>(productDto);

            // Check if user has placed a order with this product
            var orderResponse = await client.GetAsync($"api/order/user/{userId}");
            if (!orderResponse.IsSuccessStatusCode)
            {
                ViewData["HasOrdered"] = false; // Assume false on error
            }
            else
            {
                var orders = await orderResponse.Content.ReadFromJsonAsync<List<OrderDTO>>();
                bool hasOrdered = orders?.Any(c => c.ProductId == id) ?? false;
                ViewData["HasOrdered"] = hasOrdered;
            }

            return View(product);
        }

        // GET: UserView/PlaceOrder/5
        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PlaceOrder(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"api/product/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var productDto = await response.Content.ReadFromJsonAsync<ProductDTO>();
            if (productDto == null)
                return NotFound();

            var product = _mapper.Map<ProductVM>(productDto);

            var vm = new OrderVM
            {
                ProductId = product.Id,
                ProductName = $"{product.Name}"
            };

            return View(vm);
        }

        // POST: UserView/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PlaceOrder(OrderVM model)
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (!ModelState.IsValid)
                return View(model);

            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            // Get user by email to find userId
            var userResponse = await client.GetAsync($"api/users/byemail/{email}");
            if (!userResponse.IsSuccessStatusCode)
                return Unauthorized();

            var user = await userResponse.Content.ReadFromJsonAsync<UserDTO>();
            if (user == null)
                return Unauthorized();

            // Prepare DTO
            var orderDto = _mapper.Map<OrderDTO>(model);
            orderDto.UserId = user.Id;
            orderDto.UserName = user.Name;
            orderDto.OrderedAt = DateTime.UtcNow;

            var postResponse = await client.PostAsJsonAsync("api/order/create", orderDto);

            if (!postResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Response: {postResponse.StatusCode}");

                ModelState.AddModelError("", "Failed to place order. Please try again later.");
                
                return View(model);
            }

            return RedirectToAction("MyOrders", "OrderView");
        }
    }

}
