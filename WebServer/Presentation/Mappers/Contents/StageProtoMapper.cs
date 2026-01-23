using Application.Contents.Stages;
using Contracts.Protos;

namespace WebServer.Mappers.Contents
{
    public class StageProtoMapper
    {
        public static StagePb ToProto(StageDetailDto dto)
        {
            var p = new StagePb
            {
                Id = dto.Id,
                Chapter = dto.Chapter,
                Order = dto.Order,
                Name = dto.Name ?? string.Empty,
                RecommendedPower = dto.RecommendedPower,
                StaminaCost = dto.StaminaCost,
                IsActive = dto.IsActive
            };

            // waves
            foreach (var w in dto.Waves)
            {
                var wp = new WavePb
                {
                    Index = w.Index,
                    BatchNum = w.batchNum,
                };

                foreach (var e in w.Enemies)
                {
                    wp.Enemies.Add(new EnemyPb
                    {
                        EnemyCharacterId = e.EnemyCharacterId,
                        Level = e.Level,
                        Slot = e.Slot,
                        AiProfile = e.AiProfile ?? string.Empty
                    });
                }

                p.Waves.Add(wp);
            }

            // drops
            foreach (var d in dto.Drops)
            {
                p.Drops.Add(new DropPb
                {
                    ItemId = d.ItemId,
                    Rate = (double)d.Rate,
                    MinQty = d.MinQty,
                    MaxQty = d.MaxQty,
                    FirstClearOnly = d.FirstClearOnly
                });
            }

            // first rewards
            foreach (var r in dto.FirstRewards)
            {
                p.FirstRewards.Add(new RewardPb
                {
                    ItemId = r.ItemId,
                    Qty = r.Qty
                });
            }

            // requirements
            foreach (var r in dto.Requirements)
            {
                p.Requirements.Add(new RequirementPb
                {
                    RequiredStageId = r.RequiredStageId ?? 0,
                    MinAccountLevel = r.MinAccountLevel ?? 0
                });
            }

            // batches
            foreach (var b in dto.Batches)
            {
                p.Batches.Add(new BatchPb
                {
                    BatchNum = b.BatchNum,
                    UnitKey = b.UnitKey ?? string.Empty,
                    EnvKey = b.EnvKey ?? string.Empty
                });
            }

            return p;
        }

        public static StageListPb ToProto(IEnumerable<StageDetailDto> list)
        {
            var sp = new StageListPb();
            foreach (var s in list)
            {
                sp.Stages.Add(ToProto(s));
            }
            return sp;
        }
    }
}
