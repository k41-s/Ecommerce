using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace WebApp.Controllers
{
    public class ProductImageController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public ProductImageController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            // If images are protected, pass the token. If public, you can remove this block.
            var token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        // GET: /ProductImage/GetImage/5
        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            var client = GetAuthenticatedClient();

            var response = await client.GetAsync($"/api/productimages/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

            return File(imageBytes, contentType);
        }

        // POST: /ProductImage/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int imageId, int productId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"/api/productimages/{imageId}");

            if (response.IsSuccessStatusCode)
            {
                // Redirect back to the Product Edit page so the user sees the update
                return RedirectToAction("Edit", "Product", new { id = productId });
            }

            return BadRequest("Could not delete image");
        }
    }
}
