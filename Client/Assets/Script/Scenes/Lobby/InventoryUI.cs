using Contracts.Protos;
using Game.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using Toggle = UnityEngine.UI.Toggle;
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    public int SelectedTypeId { get; private set; } = 0;

    [Header("Item Type")]
    public ToggleGroup ItemTypeToggleGroup;
    public ItemTypeToggleUI ItemTypeTogglePrefab;
    public Toggle[] ItemTypeToggles;

    [Header("UserInventory")]
    public ScrollRect ItemIconRect;
    public RectTransform ItemIconViewContent;
    public BagItemIconUI BagItemIcon;

    public ItemDetailUI ItemDetailUI;

    [Header("Currency")]
    public TMP_Text TokenText;
    public TMP_Text GoldText;
    public TMP_Text GemText;

    public List<BagItemIconUI> BagItemIcons = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        Set(ItemCache.Instance.ItemTypeList);
         
        SetCurrency(GameState.Instance.CurrentUser.UserProfilePb);
    }

    private void OnDisable()
    {
        if (ItemDetailUI != null)
            ItemDetailUI.gameObject.SetActive(false);
    }
    public void SetCurrency(UserProfilePb p)
    {
        TokenText.text = p.Token.ToString();
        GoldText.text = p.Gold.ToString();
        GemText.text = p.Gem.ToString();
    }
    public void Set(List<ItemTypeMessage> itemTypeList)
    {
        BuildItemTypeToggles(itemTypeList);
    }
    private void BuildItemTypeToggles(List<ItemTypeMessage> itemTypeList)
    {
        itemTypeList.Sort((a, b) =>
        {
            int cmp = a.SlotId.CompareTo(b.SlotId);
            if (cmp != 0)
                return cmp;
            return a.CreatedAt.CompareTo(b.CreatedAt);
        });

        for (int i = ItemTypeToggleGroup.transform.childCount - 1; i >= 0; i--)
        {
            var child = ItemTypeToggleGroup.transform.GetChild(i);
            Destroy(child.gameObject);
        }

        ItemTypeToggles = new Toggle[itemTypeList.Count];

        for (int i = 0; i < itemTypeList.Count; i++)
        {
            var itemType = Instantiate(ItemTypeTogglePrefab.gameObject, ItemTypeToggleGroup.transform);
            ItemTypeToggleUI itemTypeScript = itemType.GetComponent<ItemTypeToggleUI>();
            itemTypeScript.Set(itemTypeList[i].Name, ItemTypeToggleGroup, itemTypeList[i].Id);

            ItemTypeToggles[i] = itemType.GetComponent<Toggle>();
        }
        ItemTypeToggles[0].isOn = true;

    }
    public void Set_UserItem(int TypeNum)
    {
        // 자 생각해보자. 각 카테고리 별로 클릭시 아이템을 보여줘야함. 
        SelectedTypeId = TypeNum;
        ShowUserItem(SelectedTypeId);
    }
    public void ShowUserItem(int TypeNum)
    {
        var state = GameState.Instance;
        var user = state.CurrentUser;

        // 일단 안보이게 한다. 
        if (BagItemIcons.Count > 0)
        {
            foreach (BagItemIconUI item in BagItemIcons)
            {
                item.gameObject.SetActive(false);
            }
        }

        // 보여줘야하는 리스트
        if (!user.InventoryType.TryGetValue(TypeNum, out var idList) || idList == null || idList.Count == 0)
        {
            for (int i = 0; i < BagItemIcons.Count; i++) BagItemIcons[i].gameObject.SetActive(false);
            return;
        }

        int visible = 0;

        for (int i = 0; i < idList.Count; i++)
        {
            int itemId = idList[i].ItemId;

            // [1] 보유 수량 확인
            if (!user.Inventory.TryGetValue(itemId, out UserInventory inven) || inven.Count <= 0)
                continue;

            // [2] 아이템 메타 정보 (ItemCache에서)
            if (!ItemCache.Instance.ItemDict.TryGetValue(itemId, out var itemMeta))
                continue;

            // [3] 풀에서 n번째 아이콘 가져오기
            var icon = GetOrCreateIcon(visible);

            // [4] 데이터 바인딩 + 활성화
            icon.Set(itemMeta, inven.Count);
            icon.gameObject.SetActive(true);

            visible++;
        }


        // [5] 남는 아이콘은 비활성화
        for (int i = visible; i < BagItemIcons.Count; i++)
            BagItemIcons[i].gameObject.SetActive(false);
    }
    private BagItemIconUI GetOrCreateIcon(int index)
    {
        // index번째 아이콘이 없으면 생성
        if (index >= BagItemIcons.Count)
        {
            var go = Instantiate(BagItemIcon, ItemIconViewContent);
            go.gameObject.SetActive(false); // 기본은 꺼두기
            BagItemIcons.Add(go);
        }
        return BagItemIcons[index];
    }
}
