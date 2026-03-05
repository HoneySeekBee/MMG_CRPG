using Cache;
using Combat;
using Contracts.Protos;
using Game.Data;
using Game.Managers;
using Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Game.Logging;
using UnityEngine;
using UnityEngine.UI;
using WebServer.Protos;

public class BattleMapPopup : UIPopup
{
    public static BattleMapPopup Instance { get; private set; }
    [Header("UI")]
    public CombatSpeedBtn SpeedBtn;
    [SerializeField] private GameObject StartPopup;
    [SerializeField] private TMP_Text TimeText;

    [Header("Skill")]
    [SerializeField] private Transform SkillIconTr;
    [SerializeField] private SkillButton SkillPrefab;
    private readonly List<SkillButton> SkillButtons = new();
    [HideInInspector] public Dictionary<long, SkillButton> SkillButtonDic = new Dictionary<long, SkillButton>();


    [Header("Result")]
    [SerializeField] private ObjectPool slotPool;
    [SerializeField] private Transform slotParent;
    private readonly List<GameObject> _spawnedSlots = new List<GameObject>();
    [SerializeField] private GameObject FinishPopup;
    [SerializeField] private TMP_Text resultText;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    // Initial data must be provided from the caller.
    public void Set(Action fadeIn)
    {
        SkillButtonDic.Clear();
        FinishPopup.SetActive(false);
        var btn = FinishPopup.GetComponent<Button>();
        btn.onClick.RemoveListener(GoToStage);
        btn.onClick.AddListener(GoToStage);
        StartCoroutine(BattleMapManager.Instance.Set_BattleMap(fadeIn));
        StartPopup.SetActive(false);
        Init_SKillBtn();
        SpeedBtn.Set(BattleMapManager.Instance.OnClickSpeedButton);
        _ = AudioManager.Instance.PlayBGM("bgm_battle_field");
    }
    private void Init_SKillBtn()
    {
        // Disable all skill buttons (for pooling)
        foreach (var btn in SkillButtons)
            btn.gameObject.SetActive(false);
    }
    public void CreateSkillButton(int characterMasterId, long actorId, int level)
    {
        SkillButton btn = SkillButtons.Find(b => !b.gameObject.activeSelf);

        if (btn == null)
        {
            btn = Instantiate(SkillPrefab, SkillIconTr);
            SkillButtons.Add(btn);
        }
        SkillButtonDic[actorId] = btn;
        btn.gameObject.SetActive(true);

        // Find the character's first skill (assuming 1 skill per character for now)
        var skillId = CharacterCache.Instance.DetailById[characterMasterId].Skills[0].SkillId;
        var skillData = SkillCache.Instance.SkillDict[skillId];

        btn.Set(skillData, level, actorId);
    }

    public IEnumerator ShowStart()
    {
        StartPopup.SetActive(true);
        _ = AudioManager.Instance.PlaySFX("SFX_Enter");
        yield return new WaitForSeconds(1.5f);
        StartPopup.SetActive(false);
    }
    public void ShowResult(FinishCombatResponsePb data)
    {
        FinishPopup.SetActive(true);
        foreach (var go in _spawnedSlots)
            slotPool.Return(go);
        _spawnedSlots.Clear();
        if (data.Result == CombatResultPb.CombatResultWin)
        {
            resultText.text = "Victory";
            foreach (var r in data.Rewards)
            {
                if (!ItemCache.Instance.ItemDict.TryGetValue(r.ItemId, out var itemData)) continue;
                if (!MasterDataCache.Instance.IconSprites.TryGetValue(itemData.IconId, out var sprite)) continue;

                GameObject go = slotPool.Get();
                go.transform.SetParent(slotParent, false);

                var img = go.GetComponent<Image>();
                img.color = r.FirstClearReward ? Color.green : Color.white;

                ItemSlotUI slotUI = go.GetComponent<ItemSlotUI>();
                slotUI.Set(sprite);

                _spawnedSlots.Add(go);
            }
        }
        else if (data.Result == CombatResultPb.CombatResultLose)
        {
            resultText.text = "Defeat";
        }
        else { resultText.text = "Unknown"; }

        // TODO: display stars and stage clear text here
        GameLogger.Info($"Stage {data.StageId} Clear, Stars={data.Stars}, FirstClear={data.FirstClear}");

    }
    public void GoToStage()
    {
        StartCoroutine(CoGoToStage());

    }
    private IEnumerator CoGoToStage()
    {
        // [1] Unload battle scene
        yield return SceneController.Instance.UnloadAdditiveAsync(SceneController.MapSceneName);
        // [2] Return to lobby
        LobbyRootController.Instance.Show("Adventure");
    }

}
