using Domain.Entities.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Monsters
{
    public class MonsterService : IMonsterService
    {
        private readonly IMonsterRepository _repo;

        public MonsterService(IMonsterRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<MonsterDto>> GetAllAsync(CancellationToken ct = default)
        {
            var monsters = await _repo.GetAllAsync(ct);
            return monsters.Select(MapToDto).ToList();
        }

        public async Task<MonsterDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var monster = await _repo.GetByIdAsync(id, ct);
            return monster is null ? null : MapToDto(monster);
        }

        public async Task<int> CreateAsync(CreateMonsterRequest request, CancellationToken ct = default)
        {
            var monster = new Monster(
                request.Name,
                request.ModelKey,
                request.ElementId,
                request.PortraitId
            );

            if (request.Stats is not null)
            {
                foreach (var s in request.Stats)
                {
                    monster.AddOrUpdateStat(
                        s.Level,
                        s.HP,
                        s.ATK,
                        s.DEF,
                        s.SPD,
                        s.CritRate,
                        s.CritDamage
                    );
                }
            }

            await _repo.AddAsync(monster, ct);
            await _repo.SaveChangesAsync(ct);

            return monster.Id;
        }

        public async Task UpdateAsync(UpdateMonsterRequest request, CancellationToken ct = default)
        {
            var monster = await _repo.GetByIdAsync(request.Id, ct);
            if (monster is null)
                throw new KeyNotFoundException($"Monster {request.Id} not found");

            // 도메인에 Update 메서드 만들어놨다고 가정
            monster.Update(request.Name, request.ModelKey, request.ElementId, request.PortraitId);

            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var monster = await _repo.GetByIdAsync(id, ct);
            if (monster is null)
                return;

            await _repo.DeleteAsync(monster, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpsertStatAsync(UpsertMonsterStatRequest request, CancellationToken ct = default)
        {
            var monster = await _repo.GetByIdAsync(request.MonsterId, ct);
            if (monster is null)
                throw new KeyNotFoundException($"Monster {request.MonsterId} not found");

            monster.AddOrUpdateStat(
                request.Level,
                request.HP,
                request.ATK,
                request.DEF,
                request.SPD,
                request.CritRate,
                request.CritDamage
            );

            await _repo.SaveChangesAsync(ct);
        }

        private static MonsterDto MapToDto(Monster m)
        {
            return new MonsterDto
            {
                Id = m.Id,
                Name = m.Name,
                ModelKey = m.ModelKey,
                ElementId = m.ElementId,
                PortraitId = m.PortraitId,
                Stats = m.Stats
                    .OrderBy(s => s.Level)
                    .Select(s => new MonsterStatDto
                    {
                        MonsterId = s.MonsterId,
                        Level = s.Level,
                        HP = s.HP,
                        ATK = s.ATK,
                        DEF = s.DEF,
                        SPD = s.SPD,
                        CritRate = s.CritRate,
                        CritDamage = s.CritDamage
                    })
                    .ToList()
            };
        }
    } 
}