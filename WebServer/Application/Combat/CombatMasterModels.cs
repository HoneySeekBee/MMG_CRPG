using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public sealed class CombatActorDef
    {
        public int MasterId { get; }
        public bool IsPlayer { get; }
        public string ModelKey { get; }
        public int MaxHp { get; }
        public int Atk { get; }
        public int Def { get; }
        public int Spd { get; }

        public float Range { get; }          // 근 1 ~ 원 3 사이 값
        public int AttackIntervalMs { get; } // 기본 평타 딜레이

        public double CritRate { get; }
        public double CritDamage { get; }

        public CombatActorDef(
            int masterId,
            bool isPlayer,
            string modelKey,
            int maxHp,
            int atk,
            int def,
            int spd,
            float range,
            int attackIntervalMs,
            double critRate,
            double critDamage)
        {
            MasterId = masterId;
            IsPlayer = isPlayer;
            ModelKey = modelKey;
            MaxHp = maxHp;
            Atk = atk;
            Def = def;
            Spd = spd;
            Range = range;
            AttackIntervalMs = attackIntervalMs;
            CritRate = critRate;
            CritDamage = critDamage;
        }
    }

    public sealed class CombatEnemySpawn
    {
        public int Slot { get; }
        public int MonsterId { get; }
        public int Level { get; }

        public CombatEnemySpawn(int slot, int monsterId, int level)
        {
            Slot = slot;
            MonsterId = monsterId;
            Level = level;
        }
    }
    public sealed class CombatWaveDef
    {
        public int Index { get; }
        public IReadOnlyList<CombatEnemySpawn> Enemies { get; }

        public CombatWaveDef(int index, IReadOnlyList<CombatEnemySpawn> enemies)
        {
            Index = index;
            Enemies = enemies;
        }
    }
    public sealed class CombatStageDef
    {
        public int StageId { get; }
        public IReadOnlyList<CombatWaveDef> Waves { get; }

        public CombatStageDef(int stageId, IReadOnlyList<CombatWaveDef> waves)
        {
            StageId = stageId;
            Waves = waves;
        }
    } 
    public sealed class CombatMasterDataPack
    {
        public CombatStageDef Stage { get; }
        public IReadOnlyDictionary<long, CombatActorDef> Actors { get; }

        public CombatMasterDataPack(
            CombatStageDef stage,
            IReadOnlyDictionary<long, CombatActorDef> actors)
        {
            Stage = stage;
            Actors = actors;
        }
    } 
}
