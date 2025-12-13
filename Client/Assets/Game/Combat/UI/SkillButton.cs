using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebServer.Protos;

public class SkillButton : MonoBehaviour
{
    private SkillMessage SkillData;
    private SkillLevelMessage SkillLevelData;

    [SerializeField] private Image SkillIconImage;
    [SerializeField] private Image CoolTimeImage;

    private long casterActorId;
    private Button btn;

    private bool isCooling = false;
    private float cooldownSeconds = 3f;
    public void Set(SkillMessage data, int level, long actorId)
    {
        SkillData = data;
        SkillLevelData = data.Levels[level];
        casterActorId = actorId;

        if (data.IconId != 0 && MasterDataCache.Instance.IconSprites.ContainsKey(data.IconId))
        {
            SkillIconImage.sprite = MasterDataCache.Instance.IconSprites[data.IconId];
        }
        else
        {
            Debug.Log($"[SkillSet] Character {actorId}의 스킬 아이콘 {data.IconId} 없음");
        }

        btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(ClickEvent);


        // 쿨타임 오버레이 초기화
        CoolTimeImage.fillAmount = 0;
        CoolTimeImage.gameObject.SetActive(false);
    }

    private void ClickEvent()
    {
        if (isCooling)
            return;
        BattleMapManager.Instance.RequestSkill(casterActorId, SkillData.SkillId, ok =>
        {
            if (ok)
                StartCooldown(cooldownSeconds);
        });
    }
    public void StartCooldown(float coolTime)
    {
        cooldownSeconds = coolTime;
        StartCoroutine(CoCooldown());
    }

    private IEnumerator CoCooldown()
    {
        isCooling = true;

        CoolTimeImage.gameObject.SetActive(true);
        CoolTimeImage.fillAmount = 1f;
        btn.interactable = false;

        float timer = 0f;

        while (timer < cooldownSeconds)
        {
            timer += Time.deltaTime;
            CoolTimeImage.fillAmount = 1f - (timer / cooldownSeconds);
            yield return null;
        }

        CoolTimeImage.fillAmount = 0f;
        CoolTimeImage.gameObject.SetActive(false);
        btn.interactable = true;
        isCooling = false;
    }
    public void UpdateCooldownExternally(float remainSeconds)
    {
        if (!isCooling) return;

        CoolTimeImage.fillAmount = remainSeconds / cooldownSeconds;

        if (remainSeconds <= 0)
        {
            CoolTimeImage.gameObject.SetActive(false);
            btn.interactable = true;
            isCooling = false;
        }
    }
}
