using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ecommerce.core.DTOs;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMapper _mapper;

        public ProductController(IHttpClientFactory clientFactory, IMapper mapper)
        {
            _clientFactory = clientFactory;
            _mapper = mapper;
        }

        // GET: Product
        public async Task<IActionResult> Index(
            string? name = null, 
            int? categoryId = null, 
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

            var productVMs = _mapper.Map<List<ProductVM>>(productDtos);

            await PopulateDropdownsAsync();

            int totalItems = 0;
            if (response.Headers.TryGetValues("X-Total-Count", out var totalValues))
            {
                int.TryParse(totalValues.FirstOrDefault(), out totalItems);
            }

            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewData["CurrentSearch"] = name;
            ViewData["CurrentCategoryId"] = categoryId;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ProductListPartial", productVMs);

            return View(productVMs);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/product/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var productDto = await response.Content.ReadFromJsonAsync<ProductDTO>();
            var vm = _mapper.Map<ProductVM>(productDto);

            return View(vm);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();

            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVM vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(vm);
            }

            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Handle image upload here - upload to wwwroot/uploads and get relative URL
            string? imagePath = null;
            if (Request.Form.Files.Count > 0)
            {
                var imageFile = Request.Form.Files[0];
                if (imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    imagePath = "/uploads/" + uniqueFileName;
                }
            }

            // Map VM to DTO and add ImagePath
            var productDto = _mapper.Map<ProductDTO>(vm);
            productDto.ImagePath = imagePath;

            var response = await client.PostAsJsonAsync("/api/product", productDto);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Failed to create product via API.");
                await PopulateDropdownsAsync();
                return View(vm);
            }
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/product/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var productDto = await response.Content.ReadFromJsonAsync<ProductDTO>();
            var vm = _mapper.Map<ProductVM>(productDto);

            await PopulateDropdownsAsync();

            return View(vm);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductVM vm)
        {
            if (id != vm.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(vm);
            }

            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Handle image upload if provided
            string? imagePath = null;
            if (Request.Form.Files.Count > 0)
            {
                var imageFile = Request.Form.Files[0];
                if (imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    imagePath = "/uploads/" + uniqueFileName;
                }
            }

            var productDto = _mapper.Map<ProductDTO>(vm);
            productDto.ImagePath = imagePath;

            var response = await client.PutAsJsonAsync($"/api/product/{id}", productDto);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Failed to update product via API.");
                await PopulateDropdownsAsync();
                return View(vm);
            }
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/product/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var productDto = await response.Content.ReadFromJsonAsync<ProductDTO>();
            var vm = _mapper.Map<ProductVM>(productDto);

            return View(vm);
        }

        // POST: Products/Delete/5
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

            var response = await client.DeleteAsync($"/api/product/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError("", "Cannot delete this Product due to a conflict");

                var reloadResponse = await client.GetAsync($"/api/product/{id}");

                if (!reloadResponse.IsSuccessStatusCode)
                    return NotFound();

                var dto = await reloadResponse.Content.ReadFromJsonAsync<ProductDTO>();
                var vm = _mapper.Map<ProductVM>(dto);

                return View("Delete", vm);
            }
            else
            {
                ModelState.AddModelError("", "Failed to delete Product via API.");

                var reloadResponse = await client.GetAsync($"/api/product/{id}");
                if (!reloadResponse.IsSuccessStatusCode)
                    return NotFound();

                var dto = await reloadResponse.Content.ReadFromJsonAsync<ProductDTO>();
                var vm = _mapper.Map<ProductVM>(dto);

                return View("Delete", vm);
            }
        }

        private async Task PopulateDropdownsAsync()
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var categoryResponse = await client.GetAsync("/api/category");
            var countriesResponse = await client.GetAsync("/api/countries");

            var categoryDtos = categoryResponse.IsSuccessStatusCode
                ? await categoryResponse.Content.ReadFromJsonAsync<List<CategoryDTO>>()
                : new List<CategoryDTO>();

            var countryDtos = countriesResponse.IsSuccessStatusCode
                ? await countriesResponse.Content.ReadFromJsonAsync<List<CountryDTO>>()
                : new List<CountryDTO>();

            var categorys = _mapper.Map<List<CategoryVM>>(categoryDtos);
            var countries = _mapper.Map<List<CountryVM>>(countryDtos);

            ViewData["CategoryList"] = new SelectList(categorys, "Id", "Name");
            ViewData["CountryIds"] = new MultiSelectList(countries, "Id", "Name");
        }
    }
}
