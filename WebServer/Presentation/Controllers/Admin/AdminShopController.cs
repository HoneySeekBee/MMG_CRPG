using Application.Common.Models;
using Application.Shop;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/shop")]
    public sealed class AdminShopController : ControllerBase
    {
        private readonly IShopService _svc;
        public AdminShopController(IShopService svc) => _svc = svc;

        // ───── 상점 ─────

        // GET /api/shop
        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] ShopType? shopType = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var filter = new ShopListFilter(shopType, isActive, search, page, pageSize);
            return Ok(await _svc.GetAllAsync(filter, ct));
        }

        // GET /api/shop/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
        {
            try
            {
                return Ok(await _svc.GetByIdAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST /api/shop
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateShopRequest req, CancellationToken ct = default)
        {
            try
            {
                var id = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (InvalidOperationException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
        }

        // PUT /api/shop/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateShopRequest req, CancellationToken ct = default)
        {
            try
            {
                await _svc.UpdateAsync(id, req, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
        }

        // DELETE /api/shop/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                await _svc.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ───── 상품 ─────

        // GET /api/shop/{shopId}/products
        [HttpGet("{shopId:int}/products")]
        public async Task<IActionResult> GetProducts(int shopId, CancellationToken ct = default)
        {
            try
            {
                var detail = await _svc.GetByIdAsync(shopId, ct);
                return Ok(detail.Products);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST /api/shop/{shopId}/products
        [HttpPost("{shopId:int}/products")]
        public async Task<IActionResult> AddProduct(
            int shopId, [FromBody] CreateShopProductRequest req, CancellationToken ct = default)
        {
            try
            {
                var dto = await _svc.AddProductAsync(shopId, req, ct);
                return Created($"/api/shop/{shopId}/products/{dto.Id}", dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
        }

        // PUT /api/shop/{shopId}/products/{productId}
        [HttpPut("{shopId:int}/products/{productId:int}")]
        public async Task<IActionResult> UpdateProduct(
            int shopId, int productId, [FromBody] UpdateShopProductRequest req, CancellationToken ct = default)
        {
            try
            {
                await _svc.UpdateProductAsync(shopId, productId, req, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
        }

        // DELETE /api/shop/{shopId}/products/{productId}
        [HttpDelete("{shopId:int}/products/{productId:int}")]
        public async Task<IActionResult> DeleteProduct(
            int shopId, int productId, CancellationToken ct = default)
        {
            try
            {
                await _svc.DeleteProductAsync(shopId, productId, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
        }

        // ───── 구매 기록 ─────

        // GET /api/shop/purchase-logs
        [HttpGet("purchase-logs")]
        public async Task<IActionResult> GetPurchaseLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? userId = null,
            [FromQuery] int? shopProductId = null,
            [FromQuery] DateTimeOffset? from = null,
            [FromQuery] DateTimeOffset? to = null,
            CancellationToken ct = default)
        {
            var filter = new PurchaseLogFilter(userId, shopProductId, from, to, page, pageSize);
            return Ok(await _svc.GetPurchaseLogsAsync(filter, ct));
        }
    }
}
