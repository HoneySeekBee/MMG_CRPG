using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Contents
{
    public enum StageStars : short { Zero = 0, One = 1, Two = 2, Three = 3 }
    public sealed class Stage
    {
        public int Id { get; private set; }
        public int Chapter { get; private set; }
        public int StageNumber { get; private set; }
        public short RecommendedPower { get; private set; }
        public short StaminaCost { get; private set; }
        public bool IsActive { get; private set; } = true;
        public string Name { get; private set; }
        // Navigation
        public List<StageWave> Waves { get; private set; } = new();
        public List<StageDrop> Drops { get; private set; } = new();
        public List<StageFirstClearReward> FirstRewards { get; private set; } = new();
        public List<StageRequirement> Requirements { get; private set; } = new();
        public List<StageBatch> Batches { get; private set; } = new();
        public Stage(int chapter, int stageNumber, short recommendedPower, short staminaCost, bool isActive = true, string? name = null)
        {
            Chapter = chapter;
            StageNumber = stageNumber;
            RecommendedPower = recommendedPower;
            StaminaCost = staminaCost;
            IsActive = isActive;
            Name = name;
        }
        public void SetBasic(int chapter, int stageNumber, short rec, short stam, bool isActive, string? name = null)
        {
            Chapter = chapter; StageNumber = stageNumber; RecommendedPower = rec; StaminaCost = stam; IsActive = isActive; Name = name;
        }
        /// <summary>도메인 불변식 검사 (웨이브/드롭/확률/수량 등)</summary>
        public void Validate()
        {
            if (Chapter < 1) throw new DomainException("INVALID_CHAPTER", "Chapter must be >= 1");
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



}
