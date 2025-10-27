using Contracts.EquipSlots;
using Contracts.Protos;
using Game.Data;
using MMG_CRPG.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
             
            EquipSlotBtn.GetComponent<Image>().sprite = EquipedItemSprite();

            EquipSlotBtn.onClick.RemoveAllListeners();
            EquipSlotBtn.onClick.AddListener(ShowEquipWindow);
        }

        // 현재 캐릭터가 장착한 해당 부위의 UserInventory 타입 정보가 필요하다. 


        private Sprite EquipedItemSprite()
        {
            var user = GameState.Instance.CurrentUser;
            var nowCharId = UserCharacterDeatailUI.Instance.status.CharacterId;

            // 현재 캐릭이 이 슬롯에 낀 인벤토리 ID 
            long? currentInvId = EquipmentQuery.GetEquippedInventoryId(user, nowCharId, EquipSlotData.Id);
            int iconNum = EquipSlotData.IconId;
            
            if (currentInvId == null)
                return MasterDataCache.Instance.IconSprites[iconNum];

            var itemCache = ItemCache.Instance;
            int equipTypeId = itemCache.ItemTypeDictionary["EQUIP"].Id;

            var inventories = EquipmentQuery.GetUserEquipInventories(user);
            
            var filtered = inventories
               .Where(inv => inv.Id == currentInvId)
               .FirstOrDefault();

            iconNum = EquipmentQuery.GetIconIdForSlotOrDefault(user, nowCharId, EquipSlotData);

            return MasterDataCache.Instance.IconSprites[iconNum];
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