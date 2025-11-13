using Application.Combat;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Reader
{
    public sealed class EfSkillReader : ISkillReader
    {
        private readonly GameDBContext _db;

        public EfSkillReader(GameDBContext db)
        {
            _db = db;
        }
        public async Task<SkillMasterDto> GetAsync(long skillId, CancellationToken ct)
        {
            var s = await _db.Skills
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SkillId == skillId, ct);

            if (s is null)
                throw new KeyNotFoundException($"Skill {skillId} not found");

            // TODO: SkillLevels.Values에서 실제 계수/쿨다운 읽어오기
            const int defaultCooldownMs = 5000; // 5초 쿨 예시
            const float defaultCoeff = 1.0f;    // 공격력 100%

            return new SkillMasterDto(
                SkillId: s.SkillId,
                CooldownMs: defaultCooldownMs,
                Coeff: defaultCoeff
            );
        }
    }
}
