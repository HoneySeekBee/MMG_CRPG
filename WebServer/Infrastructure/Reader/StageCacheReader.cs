using Application.Combat;
using Application.Contents.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Reader
{
    public sealed class StageCacheReader : IStageReader
    {
        private readonly IStagesCache _cache;

        public StageCacheReader(IStagesCache cache)
        {
            _cache = cache;
        }

        public Task<StageDetailDto> GetAsync(long stageId, CancellationToken ct)
        {
            var dto = _cache.GetById((int)stageId);
            if (dto is null)
                throw new KeyNotFoundException($"Stage {stageId} not found");

            // IStagesCache는 sync라서 Task.FromResult로 감싸주기
            return Task.FromResult(dto);
        }
    }
}
