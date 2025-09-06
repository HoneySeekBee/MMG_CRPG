using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Combat
    {
        public long? Id { get; private set; }
        public CombatMode Mode { get; private set; }
        public long? StageId { get; private set; }

        // 리플레이때 사용함
        public long Seed { get; private set; }

        public CombatInputSnapshot Input { get; private set; }
        public CombatResult Result { get; private set; } = CombatResult.Unknown;

        public int? ClearMs { get; private set; }
        public string? BalanceVersion { get; private set; }
        public string? ClientVersion { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        private Combat() { }

        private Combat(
          CombatMode mode,
          long? stageId,
          long seed,
          CombatInputSnapshot input,
          string? balanceVersion,
          string? clientVersion,
          DateTimeOffset? createdAtUtc = null)
        {
            Mode = mode;
            StageId = stageId;
            Seed = seed;
            Input = input ?? throw new ArgumentNullException(nameof(input));
            BalanceVersion = string.IsNullOrWhiteSpace(balanceVersion) ? null : balanceVersion;
            ClientVersion = string.IsNullOrWhiteSpace(clientVersion) ? null : clientVersion;
            CreatedAt = createdAtUtc ?? DateTimeOffset.UtcNow;
        }
        public static Combat Create(
      CombatMode mode,
      long? stageId,
      long seed,
      CombatInputSnapshot input,
      string? balanceVersion,
      string? clientVersion,
      DateTimeOffset? createdAtUtc = null)
        {
            if (seed == 0) throw new ArgumentException("Seed must be non-zero.", nameof(seed));
            return new Combat(mode, stageId, seed, input, balanceVersion, clientVersion, createdAtUtc);
        }

        public void SetId(long id)
        {
            if (Id.HasValue) throw new InvalidOperationException("Id already set.");
            Id = id;
        }
        public void CompleteWin(int clearMs)
        {
            if (clearMs < 0) throw new ArgumentOutOfRangeException(nameof(clearMs));
            Result = CombatResult.Win;
            ClearMs = clearMs;
        }
        public void CompleteLose(int clearMs)
        {
            if (clearMs < 0) throw new ArgumentOutOfRangeException(nameof(clearMs));
            Result = CombatResult.Lose;
            ClearMs = clearMs;
        }
        public void CompleteError()
        {
            Result = CombatResult.Error;
            ClearMs = null;
        }
    }
    public sealed record CombatInputSnapshot(
        int StageId,
        PartyMember[] Party,
        SkillInput[] SkillInputs
    );
    public sealed record PartyMember(long CharacterId, int Level);
    public sealed record SkillInput(int TMs, string CasterRef, long SkillId, string[] Targets);



}
