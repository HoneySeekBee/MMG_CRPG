using Application.Currency;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/currencies
    public sealed class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _svc;
        public CurrenciesController(ICurrencyService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var list = await _svc.ListAsync(ct);
            return Ok(list);
        }
    }
}
