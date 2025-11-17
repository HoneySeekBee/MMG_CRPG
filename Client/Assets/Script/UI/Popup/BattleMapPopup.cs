using Combat;
using Contracts.Protos;
using Game.Managers;
using Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleMapPopup : UIPopup
{
    public static BattleMapPopup Instance { get; private set; }
    [Header("UI")]
    [SerializeField] private Button PauseBtn;
    [SerializeField] private Button SpeedBtn;
    [SerializeField] private Button AutoBtn;
    [SerializeField] private TMP_Text TimeText;
    [SerializeField] private Transform SkillIconTr;

    [SerializeField] private GameObject StartPopup;

    [SerializeField] private ObjectPool slotPool;
    [SerializeField] private Transform slotParent;
    private readonly List<GameObject> _spawnedSlots = new List<GameObject>();
    [SerializeField] private GameObject FinishPopup;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    // 이제 여기서 스테이지에 대한 정보를 받아야한다. 
    public void Set(Action fadeIn)
    {
        FinishPopup.SetActive(false);
        FinishPopup.GetComponent<Button>().onClick.AddListener(GoToStage);
        StartCoroutine(BattleMapManager.Instance.Set_BattleMap(fadeIn));
        StartPopup.SetActive(false);
    }

    public IEnumerator ShowStart()
    {
        StartPopup.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        StartPopup.SetActive(false);
    }
    public void ShowResult(FinishCombatResponsePb data)
    {
        FinishPopup.SetActive(true);
        foreach (var go in _spawnedSlots)
            slotPool.Return(go);
        foreach (var r in data.Rewards)
        {
            GameObject go = slotPool.Get();
            go.transform.SetParent(slotParent, false);

            var img = go.GetComponent<Image>();
            img.color = r.FirstClearReward ? Color.green : Color.white;

            ItemSlotUI slotUI = go.GetComponent<ItemSlotUI>();
            var iconId = ItemCache.Instance.ItemDict[r.ItemId].IconId;
            slotUI.Set(MasterDataCache.Instance.IconSprites[iconId]);

            _spawnedSlots.Add(go);
        }

        // 별 표시, 클리어 텍스트 등도 여기서
        Debug.Log($"Stage {data.StageId} Clear, Stars={data.Stars}, FirstClear={data.FirstClear}");

    }
    public void GoToStage()
    {
        StartCoroutine(CoGoToStage());

    }
    private IEnumerator CoGoToStage()
    {
        // [1] 현재 씬 비활성화
        yield return SceneController.Instance.UnloadAdditiveAsync(SceneController.MapSceneName);
        // [2] Show()
        LobbyRootController.Instance.Show("Adventure");
    }

}
