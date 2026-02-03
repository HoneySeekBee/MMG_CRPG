using Application.Currency;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/currencies")]
    public sealed class AdminCurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _svc;
        public AdminCurrenciesController(ICurrencyService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var list = await _svc.ListAsync(ct);
            return Ok(list);
        }
    }
}
