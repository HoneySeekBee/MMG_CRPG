using Application.UserCurrency;
using Contracts.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Security.Claims;
using ProtoWalletService = Contracts.Protos.WalletService;


namespace WebServer.GrpcServices
{
    public class WalletServiceGrpc : ProtoWalletService.WalletServiceBase
    {
        private readonly IWalletService _wallet;
        private readonly ICurrencyRepository _cur;

        public WalletServiceGrpc(IWalletService wallet, ICurrencyRepository cur)
        {
            _wallet = wallet;
            _cur = cur;
        }

        private int CurrentUserId(ServerCallContext ctx)
        {
            var idStr = ctx.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (idStr == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "NO_USER_ID"));

            return int.Parse(idStr);
        }

        public override async Task<UserWalletPb> Summary(Empty request, ServerCallContext context)
        {
            int userId = CurrentUserId(context);

            var balances = await _wallet.GetBalancesAsync(userId, context.CancellationToken);
            var masters = await _cur.GetAllAsync(context.CancellationToken);

            var sortMap = masters.ToDictionary(m => m.Code, m => 0);

            var pb = new UserWalletPb
            {
                UpdatedUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            foreach (var (code, amt) in balances)
            {
                pb.Balances.Add(new CurrencyBalancePb
                {
                    Code = code,
                    Amount = amt,
                    SortOrder = sortMap.TryGetValue(code, out var s) ? s : 0
                });
            }

            return pb;
        }
    }
}
