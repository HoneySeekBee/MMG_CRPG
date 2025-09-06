using Application.Repositories;
using Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Application.Combat
{
    public sealed class CombatService : ICombatService
    {
        private readonly IMasterDataProvider _md;
        private readonly ICombatRepository _repo;
        private readonly ICombatEngine _engine;
        private const int MaxPageSize = 500;

        public CombatService(
            IMasterDataProvider md,
            ICombatRepository repo,
            ICombatEngine engine)
        {
            _md = md;
            _repo = repo;
            _engine = engine;
        }

        public async Task<SimulateCombatResponse> SimulateAsync(SimulateCombatRequest req, CancellationToken ct)
        {
            // 1) 검증
            if (req.Party is null || req.Party.Count == 0)
                throw new ArgumentException("Party is required.");

            // 2) 시드 결정(0 방지)
            var seed = req.Seed ?? BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            if (seed == 0) seed = 1;

            // 3) 도메인 입력 스냅샷
            var party = req.Party
                .Select(p => new Domain.Entities.PartyMember(p.CharacterId, p.Level))
                .ToArray();

            var skills = (req.SkillInputs ?? Enumerable.Empty<SkillInputDto>())
                .Select(s => new Domain.Entities.SkillInput(s.TMs, s.CasterRef, s.SkillId, s.Targets.ToArray()))
                .ToArray();

            var input = new Domain.Entities.CombatInputSnapshot(req.StageId, party, skills);

            // 4) 마스터 데이터 패키지
            var partyIds = party.Select(x => x.CharacterId).ToArray();
            var pack = await _md.BuildPackAsync(req.StageId, partyIds, ct);

            // 5) Aggregate 생성
            var combat = Domain.Entities.Combat.Create(
                Domain.Enum.CombatMode.Pve, req.StageId, seed, input,
                balanceVersion: "1", // TODO: 운영툴/설정에서 주입
                clientVersion: req.ClientVersion);

            // 6) 전투 시뮬레이션
            var result = _engine.Simulate(input, seed, pack);

            // 7) 결과 반영
            switch (result.Result)
            {
                case Domain.Enum.CombatResult.Win: combat.CompleteWin(result.ClearMs); break;
                case Domain.Enum.CombatResult.Lose: combat.CompleteLose(result.ClearMs); break;
                default: combat.CompleteError(); break;
            }

            // 8) 저장(트랜잭션 내 combat + logs)
            var combatId = await _repo.SaveAsync(combat, result.Events, ct);

            // 9) 응답 매핑 (이벤트 일부만)  메서드 그룹 대신 람다 사용
            var eventsShort = result.Events.Take(200).Select(e => Map(e)).ToList();

            return new SimulateCombatResponse(
                combatId,
                result.Result.ToString().ToLowerInvariant(),
                combat.ClearMs,
                combat.BalanceVersion,
                combat.ClientVersion,
                eventsShort
            );
        }

        // Map 메서드는 딱 하나만 남긴다
        private static CombatLogEventDto Map(Domain.Events.CombatLogEvent e)
            => new(e.TMs, e.Type, e.Actor, e.Target, e.Damage, e.Crit, e.Extra);

        public async Task<CombatLogPageDto> GetLogAsync(long combatId, string? cursor, int size, CancellationToken ct)
        {
            if (size <= 0) size = 100;
            size = Math.Min(size, MaxPageSize);
            return await _repo.GetLogAsync(combatId, cursor, size, ct);
        }

        public Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct)
            => _repo.GetSummaryAsync(combatId, ct);

    }
}
