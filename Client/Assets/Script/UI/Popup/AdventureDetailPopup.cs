using Lobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Contracts.Protos;
using Game.Managers;
public class AdventureDetailPopup : UIPopup
{
    [Header("UI")]
    [SerializeField] private TMP_Text chapterStageText;
    [SerializeField] private TMP_Text chapterTitleText;
    [SerializeField] private Button PlayButton;

    [SerializeField] private ObjectPool slotPool;
    [SerializeField] private Transform slotParent; 
    private readonly List<GameObject> _spawnedSlots = new List<GameObject>();

    [SerializeField] private GameObject blocker;
    public void Set(StagePb data)
    {
        if (blocker != null)
            blocker.SetActive(true);
        LobbyRootController.Instance._currentStage = data;
        ClearSlots();
        chapterStageText.text = $"{data.Chapter}-{data.Order}";
        chapterTitleText.text = $"{data.Name}";

        PlayButton.onClick.RemoveAllListeners();
        PlayButton.onClick.AddListener(() => { Play(data); }); 
         
        foreach (var item in data.FirstRewards)
        {
            GameObject go = slotPool.Get();
            go.transform.SetParent(slotParent, false);      
            go.GetComponent<Image>().color = Color.green;

            ItemSlotUI slotUI = go.GetComponent<ItemSlotUI>();
            var iconId = ItemCache.Instance.ItemDict[item.ItemId].IconId;
            slotUI.Set(MasterDataCache.Instance.IconSprites[iconId]);

            _spawnedSlots.Add(go);
        }

        foreach (var item in data.Drops)
        {
            GameObject go = slotPool.Get();
            go.transform.SetParent(slotParent, false);
            go.GetComponent<Image>().color = Color.white;

            ItemSlotUI slotUI = go.GetComponent<ItemSlotUI>();
            var iconId = ItemCache.Instance.ItemDict[item.ItemId].IconId;
            slotUI.Set(MasterDataCache.Instance.IconSprites[iconId]);

            _spawnedSlots.Add(go);
        }
    }
    public void Hide()
    {
        // ½½·Ô ´Ù ¹Ý³³
        ClearSlots();
        chapterTitleText.text = string.Empty;
        chapterStageText.text = string.Empty;

        // ¹è°æ ²ô±â
        if (blocker != null)
            blocker.SetActive(false);

        gameObject.SetActive(false);
    }

    private void ClearSlots()
    {
        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            slotPool.Return(_spawnedSlots[i]);
        }
        _spawnedSlots.Clear();
    }
    private void Play(StagePb data)
    {
        Debug.Log($"{data.Chapter}-{data.Order} ½ÇÇàÇÏ±â");
        this.gameObject.SetActive(false);
        LobbyRootController.Instance.Show("PartySet");
    }
}
