using Application.Combat;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Reader
{
    public sealed class EfSkillReader : ISkillReader
    {
        private readonly CombatDbContext _db;

        public EfSkillReader(CombatDbContext db) => _db = db;

        public async Task<SkillMasterDto?> GetAsync(long skillId, CancellationToken ct)
        {
            var exists = await _db.SkillRows
                .AnyAsync(s => s.SkillId == skillId, ct);

            if (!exists)
                return null;

            // TODO: read actual cooldown/coeff from SkillLevels table
            const int defaultCooldownMs = 5000;
            const float defaultCoeff = 1.0f;

            return new SkillMasterDto(
                SkillId: skillId,
                CooldownMs: defaultCooldownMs,
                Coeff: defaultCoeff
            );
        }
    }
}
