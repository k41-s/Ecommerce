using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.core.DTOs;
using System.Net.Http.Json;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CountryController : Controller
    {
        private readonly HttpClient _client;
        private readonly IMapper _mapper;

        public CountryController(IHttpClientFactory factory, IMapper mapper)
        {
            _client = factory.CreateClient("ApiClient");
            _mapper = mapper;
        }

        // GET: Country
        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync("api/countries");
            if (!response.IsSuccessStatusCode)
                return View("Error");

            var dtos = await response.Content.ReadFromJsonAsync<List<CountryDTO>>();
            var vms = _mapper.Map<List<CountryVM>>(dtos);
            return View(vms);
        }

        // GET: Country/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var response = await _client.GetAsync($"api/countries/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var dto = await response.Content.ReadFromJsonAsync<CountryDTO>();
            var vm = _mapper.Map<CountryVM>(dto);
            return View(vm);
        }

        // GET: Country/Create
        public IActionResult Create() => View();

        // POST: Country/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CountryVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Check for existing TypeOfWork by name before creating
            var allResponse = await _client.GetAsync("/api/countries");

            var allDtos = allResponse.IsSuccessStatusCode
                ? await allResponse.Content.ReadFromJsonAsync<List<CountryDTO>>()
                : new List<CountryDTO>();

            if (allDtos.Any(t => string.Equals(t.Name, vm.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("", "An country with this name already exists.");
                return View(vm);
            }

            var dto = _mapper.Map<CountryDTO>(vm);
            var response = await _client.PostAsJsonAsync("api/countries", dto);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "An country with this name may already exist.");
            return View(vm);
        }

        // GET: Country/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _client.GetAsync($"api/countries/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var dto = await response.Content.ReadFromJsonAsync<CountryDTO>();
            var vm = _mapper.Map<CountryVM>(dto);
            return View(vm);
        }

        // POST: Country/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CountryVM vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var dto = _mapper.Map<CountryDTO>(vm);
            var response = await _client.PutAsJsonAsync($"api/countries/{id}", dto);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Another country with this name may already exist.");
            return View(vm);
        }

        // GET: Country/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _client.GetAsync($"api/countries/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var dto = await response.Content.ReadFromJsonAsync<CountryDTO>();
            var vm = _mapper.Map<CountryVM>(dto);

            return View(vm);
        }

        // POST: Country/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _client.DeleteAsync($"api/countries/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else if(response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError("", "Failed to delete country (might be linked to a product).");

                var reloadResponse = await _client.GetAsync($"/api/countries/{id}");

                if (!reloadResponse.IsSuccessStatusCode)
                    return NotFound();

                var dto = await reloadResponse.Content.ReadFromJsonAsync<CountryDTO>();
                var vm = _mapper.Map<CountryVM>(dto);

                return View("Delete", vm);
            }
            else
            {
                ModelState.AddModelError("", "Failed to delete Country via API.");
                var reloadResponse = await _client.GetAsync($"/api/countries/{id}");
                if (!reloadResponse.IsSuccessStatusCode)
                    return NotFound();

                var dto = await reloadResponse.Content.ReadFromJsonAsync<CountryDTO>();
                var vm = _mapper.Map<CountryVM>(dto);

                return View("Delete", vm);
            }
        }
    }
}
