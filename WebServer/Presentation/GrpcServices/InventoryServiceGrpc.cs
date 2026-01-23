using Contracts.Protos;
using Grpc.Core;
using Application.UserInventory;
using Google.Protobuf.WellKnownTypes;
using WebServer.Mappers;
using ProtoInv = Contracts.Protos;
using AppInv = Application.UserInventory;

using Proto = Contracts.Protos;
using App = Application.UserInventory;

namespace WebServer.GrpcServices
{

    public class InventoryServiceGrpc : Proto.InventoryService.InventoryServiceBase
    {
        private readonly IUserInventoryService _svc;

        public InventoryServiceGrpc(IUserInventoryService svc)
        {
            _svc = svc;
        }

        public override async Task<ListUserInventoryResponse> List(
            ListUserInventoryRequest request,
            ServerCallContext context)
        {
            var query = new UserInventoryListQuery(
                UserId: request.UserId,
                Page: request.Page,
                PageSize: request.PageSize,
                ItemId: request.ItemId == 0 ? null : request.ItemId,
                UpdatedFrom: request.UpdatedFromUnix > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(request.UpdatedFromUnix)
                    : null,
                UpdatedTo: request.UpdatedToUnix > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(request.UpdatedToUnix)
                    : null
            );

            var paged = await _svc.GetListAsync(query, context.CancellationToken);

            var resp = new ListUserInventoryResponse
            {
                PageInfo = new PageInfo
                {
                    Page = paged.Page,
                    PageSize = paged.PageSize,
                    TotalCount = (int)paged.TotalCount
                }
            };

            resp.Items.AddRange(paged.Items.Select(i => i.ToPb()));
            return resp;
        }

        public override async Task<ProtoInv.GrantItemResponse> Grant(ProtoInv.GrantItemRequest request, ServerCallContext context) 
        {
            var dto = await _svc.GrantAsync(
                new AppInv.GrantItemRequest(request.UserId, request.ItemId, request.Amount),
                context.CancellationToken);

            return new GrantItemResponse { Item = dto.ToPb() };
        }

        public override async Task<ProtoInv.ConsumeItemResponse> Consume(ProtoInv.ConsumeItemRequest request, ServerCallContext context)
        {
            var result = await _svc.ConsumeAsync(
                new AppInv.ConsumeItemRequest(request.UserId, request.ItemId, request.Amount),
                context.CancellationToken);

            return result.ToPb();
        }

        public override async Task<ProtoInv.SetItemCountResponse> SetItemCount( ProtoInv.SetItemCountRequest request, ServerCallContext context)
        {
            var dto = await _svc.SetCountAsync(
                new AppInv.SetItemCountRequest(request.UserId, request.ItemId, request.NewCount),
                context.CancellationToken);

            return new ProtoInv.SetItemCountResponse { Item = dto.ToPb() };
        }

        public override async Task<Google.Protobuf.WellKnownTypes.Empty> Delete(
       ProtoInv.DeleteItemRequest request, ServerCallContext context)
        {
            await _svc.DeleteAsync(
                new AppInv.DeleteItemRequest(request.UserId, request.ItemId),
                context.CancellationToken);

            return new Google.Protobuf.WellKnownTypes.Empty();
        }
    }
}
