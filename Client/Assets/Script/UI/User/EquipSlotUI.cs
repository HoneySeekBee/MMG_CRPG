using Contracts.EquipSlots;
using Contracts.Protos;
using MMG_CRPG.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebServer.Protos;

namespace MMG_CRPG.UI
{
    public class EquipSlotUI : MonoBehaviour
    {
        [SerializeField] private EquipType type;
        private EquipSlotPb EquipSlotData;
        [SerializeField] private Button EquipSlotBtn;

        public void Set()
        {
            EquipSlotData = ItemCache.Instance.EquipSlotDic[type.ToString()];

            if (EquipSlotData == null) return;
            int iconNum = EquipSlotData.IconId;
            EquipSlotBtn.GetComponent<Image>().sprite = MasterDataCache.Instance.IconSprites[iconNum];

            EquipSlotBtn.onClick.RemoveAllListeners();
            EquipSlotBtn.onClick.AddListener(ShowEquipWindow);
        }

        // 버튼을 누르면 EquipItemWindowUI가 활성화 된다. 
        public void ShowEquipWindow()
        {
            UserCharacterDeatailUI detailUI = UserCharacterDeatailUI.Instance;
            EquipItemWindowUI equipUI = detailUI.EquipUI.EquipItemWindowUI;
            equipUI.gameObject.SetActive(true);
            equipUI.Set(EquipSlotData);
        }
    }
}