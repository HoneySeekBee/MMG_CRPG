using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/SkillFxDataList")]
public class SkillFxDataList : ScriptableObject
{
    public List<SkillData> all = new();

    private Dictionary<int, SkillData> _byCharacterId;

    public void Build()
    {
        _byCharacterId = new Dictionary<int, SkillData>();
        foreach (var s in all)
        {
            if (s == null) continue;
            _byCharacterId[s.CharacterId] = s;
        }
    }

    public SkillData GetByCharacterId(int characterId)
    {
        if (_byCharacterId == null) Build();
        _byCharacterId.TryGetValue(characterId, out var data);
        return data;
    }
}
