using Application.Users;
using Contracts.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Security.Claims;
using WebServer.Mappers;
using ProtoUserService = Contracts.Protos.UserService;

namespace WebServer.GrpcServices
{
    public class UserServiceGrpc : ProtoUserService.UserServiceBase
    {
        private readonly IUserService _users;
        private readonly IUserStageProgressService _progress;

        public UserServiceGrpc(IUserService users, IUserStageProgressService progress)
        {
            _users = users;
            _progress = progress;
        }

        private int CurrentUserId(ServerCallContext ctx)
        {
            var idStr = ctx.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (idStr == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "NO_USER_ID"));

            return int.Parse(idStr);
        }

        public override async Task<UserSummaryPb> GetSummary(Empty request, ServerCallContext context)
        {
            int userId = CurrentUserId(context);
            var s = await _users.GetMySummaryAsync(userId, context.CancellationToken);

            return new UserSummaryPb
            {
                UserId = s.Id,
                Nickname = s.NickName,
                Level = s.Level,
                Gold = s.Gold,
                Gem = s.Gem,
                Token = s.Token,
                IconId = s.IconId ?? 0
            };
        }

        public override async Task<UserProfilePb> GetProfile(Empty request, ServerCallContext context)
        {
            int userId = CurrentUserId(context);
            var p = await _users.GetProfileAsync(userId, context.CancellationToken);

            return new UserProfilePb
            {
                Id = p.Id,
                UserId = p.UserId,
                Nickname = p.NickName,
                Level = p.Level,
                Exp = p.Exp,
                Gold = p.Gold,
                Gem = p.Gem,
                Token = p.Token,
                IconId = p.IconId ?? 0
            };
        }

        public override async Task<UserProfilePb> UpdateProfile(UpdateProfilePb request, ServerCallContext context)
        {
            int userId = CurrentUserId(context);

            var updated = await _users.UpdateProfileAsync(
                userId,
                new UpdateProfileRequest(request.Nickname, request.IconId == 0 ? null : request.IconId),
                context.CancellationToken);

            return new UserProfilePb
            {
                Id = updated.Id,
                UserId = updated.UserId,
                Nickname = updated.NickName,
                Level = updated.Level,
                Exp = updated.Exp,
                Gold = updated.Gold,
                Gem = updated.Gem,
                Token = updated.Token,
                IconId = updated.IconId ?? 0,
            };
        }

        public override async Task<MyStageProgressListPb> GetMyStages(Empty request, ServerCallContext context)
        {
            int userId = CurrentUserId(context);
            var list = await _progress.GetMyProgressAsync(userId, context.CancellationToken)
                       ?? new List<Domain.Entities.User.UserStageProgress>();

            return list.ToListProto();
        }

        public override async Task<Empty> ChangePassword(ChangePasswordPb request, ServerCallContext context)
        {
            int userId = CurrentUserId(context);

            await _users.ChangePasswordAsync(
                userId,
                new ChangePasswordRequest(request.OldPassword, request.NewPassword),
                context.CancellationToken);

            return new Empty();
        }
    }
}
