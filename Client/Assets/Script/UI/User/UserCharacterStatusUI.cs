using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Contracts.Protos;
using WebServer.Protos;
using Unity.VisualScripting;

public class UserCharacterStatusUI : MonoBehaviour
{
    [Header("Character Profile")]
    [SerializeField] private Image PortraitsImage;
    [SerializeField] private TMP_Text profileStart_text;
    [SerializeField] private TMP_Text profileName_text;
    [SerializeField] private Image profileFactionImage;
    [SerializeField] private Image profileRoleImage;
    [SerializeField] private Image profileElementImage;

    [Header("Character Status")]
    [SerializeField] private TMP_Text level_text;
    [SerializeField] private TMP_Text battlePower_text;
    [SerializeField] private TMP_Text hp_text;
    [SerializeField] private TMP_Text atk_text;
    [SerializeField] private TMP_Text def_text;
    [SerializeField] private TMP_Text spd_text;
    [SerializeField] private TMP_Text criRate_text;
    [SerializeField] private TMP_Text criDamage_text;
    [SerializeField] private Image[] star_imgs;

    const string YellowStarKey = "StarYellow";
    const string GrayStarKey = "StarGray";

    public void Set(UserCharacterSummaryPb thisCharacter)
    {
        CharacterDetailPb characterDetail = CharacterCache.Instance.DetailById[thisCharacter.CharacterId];
        MasterDataCache masterData = MasterDataCache.Instance;

        PortraitsImage.sprite = masterData.PortraitSprites[characterDetail.PortraitId ?? 0];
        profileStart_text.text = masterData.RarityDictionary[characterDetail.RarityId].Stars.ToString();
        profileName_text.text = characterDetail.Name;
        profileFactionImage.sprite = masterData.IconSprites[masterData.FactionDictionary[characterDetail.FactionId].IconId];
        profileRoleImage.sprite = masterData.IconSprites[masterData.RoleDictionary[characterDetail.RoleId].IconId];
        profileElementImage.sprite = masterData.IconSprites[masterData.ElementDictionary[characterDetail.ElementId].IconId];

        level_text.text = $"Lv.{thisCharacter.Level}";

        CharacterStatProgressionPb thisStat = characterDetail.StatProgressions[thisCharacter.Level];
        battlePower_text.text = Compute(thisStat.Hp, thisStat.Atk, thisStat.Def, thisStat.Spd, (float)thisStat.CritRate, (float)thisStat.CritDamage).ToString();
        hp_text.text = thisStat.Hp.ToString();
        atk_text.text = thisStat.Atk.ToString();
        def_text.text = thisStat.Def.ToString();  
        spd_text.text = thisStat.Spd.ToString();
        criRate_text.text = thisStat.CritRate.ToString();
        criDamage_text.text = thisStat.CritDamage.ToString();

        Sprite yellowStar = UIImageCache.Instance.Get(YellowStarKey);
        Sprite grayStar = UIImageCache.Instance.Get(GrayStarKey);

        Debug.Log($"노란 별 {yellowStar == null}");
        for(int i = 0; i < star_imgs.Length; i++)
        {
            star_imgs[i].sprite = (i < masterData.RarityDictionary[characterDetail.RarityId].Stars)? yellowStar : grayStar;
        }
    }

    // 임시 전투력 
    public static float Compute(
        float hp, float atk, float def, float spd,
        float critRate , float critDamage ,
        float spd0 = 100f, float kDef = 1000f, float scale = 1f)
    {
        // 안전 클램프
        critRate = Mathf.Clamp01(critRate);
        critDamage = Mathf.Max(0f, critDamage);
        spd = Mathf.Max(1f, spd);
        atk = Mathf.Max(0f, atk);
        hp = Mathf.Max(1f, hp);
        def = Mathf.Max(0f, def);

        // 1) 평균 치명타 배수
        float mCrit = 1f + critRate * critDamage;

        // 2) 유효 공격력(턴 가중)
        float ea = atk * mCrit * (spd / spd0);

        // 3) 유효 체력(EHP)
        float dr = def / (def + kDef);   // 0~1
        float ehp = hp * (1f + dr);

        // 4) 전투력
        float cpRaw = Mathf.Sqrt(ea * ehp);

        // 5) 표기용 스케일
        return Mathf.Round(cpRaw * scale);
    }

}
