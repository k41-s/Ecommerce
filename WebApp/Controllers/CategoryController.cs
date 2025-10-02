using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.core.DTOs;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMapper _mapper;

        public CategoryController(IHttpClientFactory clientFactory, IMapper mapper)
        {
            _clientFactory = clientFactory;
            _mapper = mapper;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var response = await client.GetAsync("/api/category");
            if (!response.IsSuccessStatusCode)
            {
                return View(new List<CategoryVM>());
            }

            var dtoList = await response.Content.ReadFromJsonAsync<List<CategoryDTO>>();
            var vmList = _mapper.Map<List<CategoryVM>>(dtoList);

            return View(vmList);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/category/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var dto = await response.Content.ReadFromJsonAsync<CategoryDTO>();
            var vm = _mapper.Map<CategoryVM>(dto);

            return View(vm);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var client = _clientFactory.CreateClient("ApiClient");

            // Check for existing Category by name before creating
            var allResponse = await client.GetAsync("/api/category");

            var allDtos = allResponse.IsSuccessStatusCode
                ? await allResponse.Content.ReadFromJsonAsync<List<CategoryDTO>>()
                : new List<CategoryDTO>();

            if (allDtos.Any(t => string.Equals(t.Name, vm.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("", "A category with this name already exists.");
                return View(vm);
            }

            var dto = _mapper.Map<CategoryDTO>(vm);

            var response = await client.PostAsJsonAsync("/api/category", dto);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Failed to create Category via API.");
                return View(vm);
            }
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/category/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var dto = await response.Content.ReadFromJsonAsync<CategoryDTO>();
            var vm = _mapper.Map<CategoryVM>(dto);

            return View(vm);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryVM vm)
        {
            if (id != vm.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(vm);

            var client = _clientFactory.CreateClient("ApiClient");

            // Check for duplicates
            var allResponse = await client.GetAsync("/api/category");

            var allDtos = allResponse.IsSuccessStatusCode
                ? await allResponse.Content.ReadFromJsonAsync<List<CategoryDTO>>()
                : new List<CategoryDTO>();

            if (allDtos.Any(t => t.Id != id && string.Equals(t.Name, vm.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("", "Another category with this name already exists.");
                return View(vm);
            }

            var dto = _mapper.Map<CategoryDTO>(vm);

            var response = await client.PutAsJsonAsync($"/api/category/{id}", dto);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Failed to update Category via API.");
                return View(vm);
            }
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/category/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var dto = await response.Content.ReadFromJsonAsync<CategoryDTO>();
            var vm = _mapper.Map<CategoryVM>(dto);

            return View(vm);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/category/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError("", "Cannot delete this Category because it has related Products.");

                var reloadResponse = await client.GetAsync($"/api/category/{id}");

                if (!reloadResponse.IsSuccessStatusCode)
                    return NotFound();

                var dto = await reloadResponse.Content.ReadFromJsonAsync<CategoryDTO>();
                var vm = _mapper.Map<CategoryVM>(dto);

                return View("Delete", vm);
            }
            else
            {
                ModelState.AddModelError("", "Failed to delete Category via API.");
                var reloadResponse = await client.GetAsync($"/api/category/{id}");
                if (!reloadResponse.IsSuccessStatusCode)
                    return NotFound();

                var dto = await reloadResponse.Content.ReadFromJsonAsync<CategoryDTO>();
                var vm = _mapper.Map<CategoryVM>(dto);

                return View("Delete", vm);
            }
        }
    }
}
