using Contracts.EquipSlots;
using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebServer.Protos;

public class EquipSlotUI : MonoBehaviour
{
    [SerializeField] private string CodeName; 
    private EquipSlotPb EquipSlotData;
    [SerializeField] private Button EquipSlotBtn;

    public void Set()
    {
        EquipSlotData = ItemCache.Instance.EquipSlotDic[CodeName];

        if (EquipSlotData == null) return; 
        int iconNum = EquipSlotData.IconId;
        EquipSlotBtn.GetComponent<Image>().sprite = MasterDataCache.Instance.IconSprites[iconNum];

    }
}
