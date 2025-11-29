using System.Net.Http.Headers;
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

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");

            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private async Task<bool> UploadImageToApi(int productId, IFormFile file)
        {
            var client = GetAuthenticatedClient();
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(streamContent, "file", file.FileName);

            var response = await client.PostAsync($"/api/productimages/upload/{productId}", content);
            return response.IsSuccessStatusCode;
        }

        // GET: Product
        public async Task<IActionResult> Index(
            string? name = null, 
            int? categoryId = null, 
            int page = 1)
        {
            int pageSize = 10;

            var client = GetAuthenticatedClient();

            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrEmpty(name))
                queryParams.Add($"name={Uri.EscapeDataString(name)}");

            if (categoryId.HasValue && categoryId > 0)
                queryParams.Add($"categoryId={categoryId}");

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
            var client = GetAuthenticatedClient();

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

            var client = GetAuthenticatedClient();

            // Make product here with metadata only
            var productDto = _mapper.Map<ProductDTO>(vm);

            var response = await client.PostAsJsonAsync("/api/product", productDto);
            if (response.IsSuccessStatusCode)
            {
                var createdProduct = await response.Content.ReadFromJsonAsync<ProductDTO>();

                if (vm.NewImages != null && createdProduct != null)
                {
                    foreach (var file in vm.NewImages)
                    {
                        if (file.Length > 0)
                        {
                            await UploadImageToApi(createdProduct.Id, file);
                        }
                    }
                }

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
            var client = GetAuthenticatedClient();
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

            var client = GetAuthenticatedClient();

            var productDto = _mapper.Map<ProductDTO>(vm);

            var response = await client.PutAsJsonAsync($"/api/product/{id}", productDto);

            if (response.IsSuccessStatusCode)
            {
                if (vm.NewImages != null)
                {
                    foreach (var file in vm.NewImages)
                    {
                        if (file.Length > 0)
                        {
                            await UploadImageToApi(id, file);
                        }
                    }
                }
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
            var client = GetAuthenticatedClient();

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
            var client = GetAuthenticatedClient();

            var response = await client.DeleteAsync($"/api/product/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Failed to delete product via API.");
            return RedirectToAction(nameof(Delete), new { id });
        }

        private async Task PopulateDropdownsAsync()
        {
            var client = GetAuthenticatedClient();

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
