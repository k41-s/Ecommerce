using AutoMapper;
using Ecommerce.core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize]
    public class OrderViewController : Controller
    {
        private readonly HttpClient _client;
        private readonly IMapper _mapper;

        public OrderViewController(IHttpClientFactory factory, IMapper mapper)
        {
            _client = factory.CreateClient("ApiClient");

            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UsersWithOrders()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _client.GetAsync("api/users/with-orders");
            if (!response.IsSuccessStatusCode)
                return View("Error");

            var dtoList = await response.Content.ReadFromJsonAsync<List<UserWithOrdersDTO>>();
            var vmList = _mapper.Map<List<UserWithOrdersVM>>(dtoList);
            return View(vmList);
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyOrders()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var userResponse = await _client.GetAsync($"api/users/byemail/{email}");
            if (!userResponse.IsSuccessStatusCode)
                return Unauthorized();

            var user = await userResponse.Content.ReadFromJsonAsync<UserDTO>();

            if (user == null)
            {
                return Unauthorized();
            }

            var response = await _client.GetAsync($"api/order/user/{user.Id}");

            if (!response.IsSuccessStatusCode)
                return View("Error");

            var dtoList = await response.Content.ReadFromJsonAsync<List<OrderDTO>>();
            var vms = _mapper.Map<List<MyOrderVM>>(dtoList);

            return View(vms);
        }
    }
}
