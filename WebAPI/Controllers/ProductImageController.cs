using Ecommerce.core.DTOs;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/productimages")]
    [ApiController]
    public class ProductImageController : ControllerBase
    {
        private readonly EcommerceContext _context;

        public ProductImageController(EcommerceContext context)
        {
            _context = context;
        }

        // GET: api/productimages/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductImage(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);

            if (productImage == null)
            {
                return NotFound();
            }

            var contentType = productImage.MimeType;
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = "image/png";
            }

            return File(productImage.Data, productImage.MimeType);
        }

        // POST: api/productimages/5 (where 5 is the ProductId)
        [HttpPost("upload/{productId}")]
        public async Task<ActionResult<ProductImageDTO>> UploadImage(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound($"Product with ID {productId} not found.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var productImage = new ProductImage
            {
                ProductId = productId,
                Product = product,
                Data = memoryStream.ToArray(),
                MimeType = file.ContentType
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            string? url = Url.Action(
                action: nameof(GetProductImage),
                controller: "ProductImage",
                values: new { id = productImage.Id },
                protocol: Request.Scheme);

            var dto = new ProductImageDTO
            {
                Id = productImage.Id,
                MimeType = productImage.MimeType,
                Url = url
            };

            return CreatedAtAction(nameof(GetProductImage), new { id = productImage.Id }, dto);
        }

        // DELETE: api/productimages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage == null)
            {
                return NotFound();
            }

            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
