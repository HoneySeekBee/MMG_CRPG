using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Contracts.Protos;
using UnityEngine.Events;
public class BagItemIconUI : MonoBehaviour
{
    [SerializeField] private bool isEquip = false;
    private Button BagItemIconBtn;
    public ItemMessage ItemData;
    private UserInventory userInventoryData;
    public TMP_Text countText;
    public Image IconImage;

    public void Set(ItemMessage _itemData, UserInventory inv)
    {
        ItemData = _itemData;
        IconImage.sprite = MasterDataCache.Instance.IconSprites[ItemData.IconId];
        if (countText != null && countText.gameObject.activeSelf)
            countText.text = inv.Count.ToString();
        BagItemIconBtn = this.GetComponent<Button>();
        BagItemIconBtn.onClick.RemoveAllListeners();
        userInventoryData = inv;
        if (isEquip == false)
            BagItemIconBtn.onClick.AddListener(ShowDetail);

    }

    public void AddClickEvent(UnityAction<ItemMessage, UserInventory> action)
    {
        BagItemIconBtn.onClick.AddListener(() =>
            action?.Invoke(ItemData, userInventoryData));
    }

    public void ShowDetail()
    {
        ItemDetailUI ItemDetail = InventoryUI.Instance.ItemDetailUI;

        if (ItemDetail.gameObject.activeSelf == false)
            ItemDetail.gameObject.SetActive(true);
        ItemDetail.Set(ItemData);
    }
}
