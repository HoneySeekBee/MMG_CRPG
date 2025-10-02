using Contracts.Protos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Item Type")]
    public ToggleGroup ItemTypeToggleGroup;
    public ItemTypeToggleUI ItemTypeToggle;

    // 1. 카테고리 목록을 받아온다.

    // 2. 해당 유저의 아이템 목록을 받아온다. 
    public void Set(List<ItemTypeMessage> itemTypeList)
    {
        Set_ItemCategory(itemTypeList);
    }
    private void Set_ItemCategory(List<ItemTypeMessage> itemTypeList)
    {
        itemTypeList.Sort((a, b) =>
        {
            int cmp = a.SlotId.CompareTo(b.SlotId);
            if (cmp != 0)
                return cmp;
            return a.CreatedAt.CompareTo(b.CreatedAt);
        });


        foreach (ItemTypeMessage item in itemTypeList)
        {
            var itemType = Instantiate(ItemTypeToggle.gameObject, ItemTypeToggleGroup.gameObject.transform);
            ItemTypeToggleUI itemTypeScript = itemType.GetComponent<ItemTypeToggleUI>();
            itemTypeScript.Set(item.Name, ItemTypeToggleGroup);
        }
    }
}
