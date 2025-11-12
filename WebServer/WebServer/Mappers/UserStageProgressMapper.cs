using Contracts.Protos;
using Domain.Entities.Contents;
using Domain.Entities.User;

namespace WebServer.Mappers
{
    public static class UserStageProgressMapper
    {
        public static UserStageProgressPb ToProto(this UserStageProgress p)
        {
            return new UserStageProgressPb
            {
                StageId = p.StageId,
                Cleared = p.Cleared,
                Stars = p.Stars switch
                {
                    StageStars.Zero => StageStarsPb.StageStarsZero,
                    StageStars.One => StageStarsPb.StageStarsOne,
                    StageStars.Two => StageStarsPb.StageStarsTwo,
                    StageStars.Three => StageStarsPb.StageStarsThree,
                    _ => StageStarsPb.StageStarsZero
                },
                // DateTime? -> epoch(ms)
                ClearedAtUtc = p.ClearedAt is null
                    ? 0
                    : new DateTimeOffset(p.ClearedAt.Value).ToUnixTimeMilliseconds()
            };
        }

        public static MyStageProgressListPb ToListProto(this IEnumerable<UserStageProgress> list)
        {
            var pb = new MyStageProgressListPb();
            foreach (var x in list)
                pb.Progresses.Add(x.ToProto());
            return pb;
        }
    }
}
