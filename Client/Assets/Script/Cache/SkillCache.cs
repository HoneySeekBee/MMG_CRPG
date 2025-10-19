using Game.Core;
using Game.Network;
using Game.UICommon;
using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WebServer.Protos;

public class SkillCache : MonoBehaviour
{
    public static SkillCache Instance { get; private set; }

    [Header("SkillData")]
    public Dictionary<int, SkillMessage> SkillDict = new();

    public long Version { get; private set; } = 0;
    private SkillsResponse _skills;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator CoLoadSkillData(ProtoHttpClient http, Popup popup)
    { 
        SkillDict.Clear();

        yield return http.Get(ApiRoutes.Skills, SkillsResponse.Parser,
            (ApiResult<SkillsResponse> res) =>
            {
                if (!res.Ok)
                {
                    popup?.Show($"스킬 캐시 받아오기 실패 : {res.Message}");
                    Debug.Log($"스킬 캐시 받아오기 실패 : {res.Message}");
                    return;
                }
                _skills = res.Data;
            });
        
        if (_skills == null || _skills.Skills.Count == 0)
        {
            Debug.Log($"[ FAILED ] - Load Skill Cache. CHECK : {_skills == null}");
            yield break;
        }

        SkillDict = new Dictionary<int, SkillMessage>(_skills.Skills.Count);

        Version = _skills.Version;

        foreach (var skill in _skills.Skills)
        {
            SkillDict[skill.SkillId] = skill;
        }
        Debug.Log($"[Skill - Cache] : count {_skills.Skills.Count}");
    }
    public bool TryGetSkill(int skillId, out SkillMessage skill) => SkillDict.TryGetValue(skillId, out skill);

    public static string StructToJson(Struct s) => s == null ? "{}" : s.ToString();
}
