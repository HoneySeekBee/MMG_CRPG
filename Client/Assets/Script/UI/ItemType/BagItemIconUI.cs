using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Contracts.Protos;
public class BagItemIconUI : MonoBehaviour
{
    private Button BagItemIconBtn; 
    public ItemMessage ItemData;
    public TMP_Text countText;
    public Image IconImage;

    public void Set(ItemMessage _itemData , int count)
    {
        ItemData = _itemData;
        IconImage.sprite = MasterDataCache.Instance.IconSprites[ItemData.IconId];
        countText.text = count.ToString();
        BagItemIconBtn = this.GetComponent<Button>();
        BagItemIconBtn.onClick.RemoveAllListeners();
        BagItemIconBtn.onClick.AddListener(ShowDetail);
    }

    public void ShowDetail()
    {
        ItemDetailUI ItemDetail = InventoryUI.Instance.ItemDetailUI;

        if (ItemDetail.gameObject.activeSelf == false)
            ItemDetail.gameObject.SetActive(true);
        ItemDetail.Set(ItemData);
    }
}
