using Application.Repositories;
using Application.UserCurrency;
using Contracts.Protos;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebServer.Controllers
{
    [Authorize, ApiController]
    [Route("api/pb/me/wallet")]
    [Produces("application/x-protobuf")]
    public sealed class MeWalletProtoController : ControllerBase
    {
        private readonly IWalletService _wallet;
        private readonly ICurrencyRepository _cur;
        private readonly IClock _clock;

        public MeWalletProtoController(IWalletService w, ICurrencyRepository cur, IClock clock)
        { _wallet = w; _cur = cur; _clock = clock; }

        private int CurrentUserId() =>
            int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        [HttpGet("summary")]
        [Produces("application/x-protobuf")]
        public async Task<ActionResult<UserWalletPb>> Summary(CancellationToken ct)
        {
            var list = await _wallet.GetBalancesAsync(CurrentUserId(), ct); 
            var masters = await _cur.GetAllAsync(ct);

            var sortMap = masters.ToDictionary(m => m.Code, m => 0 );

            var pb = new UserWalletPb { UpdatedUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };

            foreach (var (code, amount) in list)
            {
                pb.Balances.Add(new CurrencyBalancePb
                {
                    Code = code,
                    Amount = amount,
                    SortOrder = sortMap.TryGetValue(code, out var s) ? s : 0
                });
            }

            return Ok(pb);
        }
    }
}
