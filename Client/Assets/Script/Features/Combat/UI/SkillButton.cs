using System;
using System.Collections;
using System.Collections.Generic;
using Game.Logging;
using UnityEngine;
using UnityEngine.UI;
using WebServer.Protos;

public class SkillButton : MonoBehaviour
{
    private bool isAllive;
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
        CharacterAllive();
        SkillData = data;
        SkillLevelData = data.Levels[level];
        casterActorId = actorId;

        if (data.IconId != 0 && MasterDataCache.Instance.IconSprites.ContainsKey(data.IconId))
        {
            SkillIconImage.sprite = MasterDataCache.Instance.IconSprites[data.IconId];
        }
        else
        {
            GameLogger.Warn($"[SkillButton] Icon not found for actor={actorId}, iconId={data.IconId}");
        }

        btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(ClickEvent);

        // Reset cooldown UI
        CoolTimeImage.fillAmount = 0;
        CoolTimeImage.gameObject.SetActive(false);
    }
    public void CharacterAllive()
    {
        isAllive = true;
        SkillIconImage.color = Color.white;
    }
    public void CharacterDead()
    {
        isAllive = false;
        SkillIconImage.color = Color.gray;
    }

    private void ClickEvent()
    {
        if (isAllive == false)
            return;
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
