using PixPlays.ElementalVFX;
using UnityEngine;
using WebServer.Protos;

[CreateAssetMenu(menuName = "Game/SkillData")]
public class SkillData : ScriptableObject
{
    public int CharacterId;

    [Header("스킬 FX (0돌 / 1돌 / 2돌)")]
    public SkillFxSet[] fxByBreakthrough = new SkillFxSet[3];

    [Header("평타 FX (기본/크리티컬)")]
    public SkillFxSet normalAttackFx;
    public SkillFxSet criticalAttackFx;

    [Header("무기 궤적 FX")]
    public SkillFxSet weaponTrailFx;

    public SkillFxSet GetFxSet(int breakthroughLevel)
    {
        breakthroughLevel = Mathf.Clamp(breakthroughLevel, 0, fxByBreakthrough.Length - 1);
        return fxByBreakthrough[breakthroughLevel];
    }
}
[System.Serializable]
public class SkillFxSet
{
    public int skillId;
    public string skillName;
    public BaseVfx skillFx;
    public AudioClip castSound;
    public AudioClip hitSound;
}