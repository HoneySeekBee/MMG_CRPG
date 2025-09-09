using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public enum StageStars : short { Zero = 0, One = 1, Two = 2, Three = 3 }
    public sealed class Stage
    {
        public int Id { get; private set; }
        public int Chapter { get; private set; }
        public int Order { get; private set; }
        public string? Name { get; private set; }
        public short RecommendedPower { get; private set; }
        public short StaminaCost { get; private set; }
        public bool IsActive { get; private set; } = true;

        public void SetName(string? name) => Name = name;
        // Navigation
        public List<StageWave> Waves { get; private set; } = new();
        public List<StageDrop> Drops { get; private set; } = new();
        public List<StageFirstClearReward> FirstRewards { get; private set; } = new();
        public List<StageRequirement> Requirements { get; private set; } = new();

        public Stage(int chapter, int order, short recommendedPower, short staminaCost, bool isActive = true)
        {
            Chapter = chapter;
            Order = order;
            RecommendedPower = recommendedPower;
            StaminaCost = staminaCost;
            IsActive = isActive;
        }
        public void SetBasic(int chapter, int order, short rec, short stam, bool isActive, string? name = null)
        {
            Chapter = chapter; Order = order; RecommendedPower = rec; StaminaCost = stam; IsActive = isActive; Name = name;
        }
        /// <summary>도메인 불변식 검사 (웨이브/드롭/확률/수량 등)</summary>
        public void Validate()
        {
            if (Chapter < 1) throw new DomainException("INVALID_CHAPTER", "Chapter must be >= 1");
            if (Order < 1) throw new DomainException("INVALID_ORDER", "Order must be >= 1");
            if (RecommendedPower < 0) throw new DomainException("INVALID_RECOMMENDED_POWER", "RecommendedPower >= 0");
            if (StaminaCost < 0) throw new DomainException("INVALID_STAMINA_COST", "StaminaCost >= 0");

            if (Waves.Count == 0) throw new DomainException("INVALID_WAVES", "At least one wave is required.");
            foreach (var w in Waves) w.Validate();

            var rateSum = Drops.Sum(d => d.Rate);
            if (rateSum > 1.0m + 0.00001m) // 부동소수 여지
                throw new DomainException("INVALID_DROPS", $"Drop rate sum ≤ 1.0 (current: {rateSum}).");

            foreach (var d in Drops) d.Validate();
            foreach (var r in Requirements) r.Validate();
            foreach (var r in FirstRewards) r.Validate();
        }
    }

    public sealed class StageWave
    {
        public int Id { get; private set; }
        public int StageId { get; private set; }
        public short Index { get; private set; } // 1..N
        public List<StageWaveEnemy> Enemies { get; private set; } = new();

        public StageWave(short index) => Index = index;

        public void Validate()
        {
            if (Index < 1) throw new DomainException("INVALID_WAVE_INDEX", "Wave index must be ≥ 1");
            if (Enemies.Count == 0) throw new DomainException("INVALID_WAVE_ENEMIES", "Wave must have at least one enemy.");
            foreach (var e in Enemies) e.Validate();
            var duplicateSlot = Enemies.GroupBy(e => e.Slot).FirstOrDefault(g => g.Count() > 1);
            if (duplicateSlot != null)
                throw new DomainException("DUPLICATE_SLOT", $"Slot {duplicateSlot.Key} appears more than once.");
        }
    }

    public sealed class StageWaveEnemy
    {
        public int Id { get; private set; }
        public int StageWaveId { get; private set; }
        public int EnemyCharacterId { get; private set; }
        public short Level { get; private set; }  // ≥1
        public short Slot { get; private set; }   // 1..6/9
        public string? AiProfile { get; private set; }

        public StageWaveEnemy(int enemyCharacterId, short level, short slot, string? aiProfile = null)
        {
            EnemyCharacterId = enemyCharacterId;
            Level = level;
            Slot = slot;
            AiProfile = aiProfile;
        }

        public void Validate()
        {
            if (EnemyCharacterId <= 0) throw new DomainException("INVALID_ENEMY", "EnemyCharacterId required.");
            if (Level < 1) throw new DomainException("INVALID_LEVEL", "Level must be ≥ 1");
            if (Slot < 1 || Slot > 9) throw new DomainException("INVALID_SLOT", "Slot must be between 1 and 9.");
        }
    }

    public sealed class StageDrop
    {
        public int Id { get; private set; }
        public int StageId { get; private set; }
        public int ItemId { get; private set; }
        public decimal Rate { get; private set; }     // 0..1
        public short MinQty { get; private set; }     // ≥0
        public short MaxQty { get; private set; }     // ≥Min
        public bool FirstClearOnly { get; private set; }

        public StageDrop(int itemId, decimal rate, short minQty, short maxQty, bool firstClearOnly = false)
        {
            ItemId = itemId;
            Rate = rate;
            MinQty = minQty;
            MaxQty = maxQty;
            FirstClearOnly = firstClearOnly;
        }

        public void Validate()
        {
            if (ItemId <= 0) throw new DomainException("INVALID_ITEM", "ItemId required.");
            if (Rate < 0m || Rate > 1.0m) throw new DomainException("INVALID_RATE", "Rate must be between 0 and 1.");
            if (MinQty < 0) throw new DomainException("INVALID_QTY_MIN", "MinQty must be ≥ 0.");
            if (MaxQty < MinQty) throw new DomainException("INVALID_QTY_MAX", "MaxQty must be ≥ MinQty.");
        }
    }

    public sealed class StageFirstClearReward
    {
        public int Id { get; private set; }
        public int StageId { get; private set; }
        public int ItemId { get; private set; }
        public short Qty { get; private set; } // >0

        public StageFirstClearReward(int itemId, short qty)
        {
            ItemId = itemId;
            Qty = qty;
        }

        public void Validate()
        {
            if (ItemId <= 0) throw new DomainException("INVALID_ITEM", "ItemId required.");
            if (Qty <= 0) throw new DomainException("INVALID_QTY", "Qty must be > 0.");
        }
    }

    public sealed class StageRequirement
    {
        public int Id { get; private set; }
        public int StageId { get; private set; }
        public int? RequiredStageId { get; private set; }
        public short? MinAccountLevel { get; private set; }

        public StageRequirement(int? requiredStageId = null, short? minAccountLevel = null)
        {
            RequiredStageId = requiredStageId;
            MinAccountLevel = minAccountLevel;
        }

        public void Validate()
        {
            if (RequiredStageId is null && MinAccountLevel is null)
                throw new DomainException("INVALID_REQUIREMENT", "At least one requirement must be set.");
            if (MinAccountLevel is < 1)
                throw new DomainException("INVALID_ACCOUNT_LEVEL", "MinAccountLevel must be ≥ 1 when set.");
        }
    }

}
