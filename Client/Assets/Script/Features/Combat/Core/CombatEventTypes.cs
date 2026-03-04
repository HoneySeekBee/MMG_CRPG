namespace Game.Combat
{
    public static class CombatEventTypes
    {
        public const string Spawn         = "spawn";
        public const string WaveCleared   = "wave_cleared";
        public const string StageCleared  = "stage_cleared";
        public const string StageResult   = "stage_result";

        public const string NormalAttack  = "normal_attack";
        public const string Hit           = "hit";
        public const string SkillCast     = "skill_cast";
        public const string SkillHit      = "skill_hit";
        public const string SkillHitAoe   = "skill_hit_aoe";
    }
}
