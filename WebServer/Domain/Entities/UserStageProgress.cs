using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class UserStageProgress
    {
        public int UserId { get; private set; }
        public int StageId { get; private set; }
        public bool Cleared { get; private set; }
        public StageStars Stars { get; private set; }
        public DateTime? ClearedAt { get; private set; }

        public UserStageProgress(int userId, int stageId)
        {
            UserId = userId;
            StageId = stageId;
        }

        public void MarkFinish(bool success, StageStars stars, DateTime nowUtc)
        {
            if (!success)
            {
                // 실패 시 별/시간 미변경
                return;
            }

            if (stars < StageStars.Zero || stars > StageStars.Three)
                throw new DomainException("INVALID_STARS", "Stars must be between 0 and 3.");

            if (!Cleared) ClearedAt = nowUtc;
            Cleared = true;
            // 더 높은 별만 유지
            if (stars > Stars) Stars = stars;
        }
    }
}
