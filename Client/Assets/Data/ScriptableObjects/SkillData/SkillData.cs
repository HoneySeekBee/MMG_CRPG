using PixPlays.ElementalVFX;
using UnityEngine;
using WebServer.Protos;

[CreateAssetMenu(menuName = "Game/SkillData")]
public class SkillData : ScriptableObject
{
    public int CharacterId;

    [Header("Skill FX (Breakthrough 0 / 1 / 2)")]
    public SkillFxSet[] fxByBreakthrough = new SkillFxSet[3];

    [Header("Attack FX (Normal / Critical)")]
    public SkillFxSet normalAttackFx;
    public SkillFxSet criticalAttackFx;

    [Header("Weapon Trail FX")]
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
    [Tooltip("VFX scale multiplier (1 = default, 0.5 = half, 2 = double)")]
    public float fxScale = 1f;
    public AudioClip castSound;
    public AudioClip hitSound;
}
