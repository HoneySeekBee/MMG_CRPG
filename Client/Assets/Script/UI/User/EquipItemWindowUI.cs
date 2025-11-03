using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Contracts.EquipSlots;
using Game.Data;
using Contracts.Protos;
using Game.Core;
using Game.MasterData;
using Google.Protobuf.WellKnownTypes;
using static System.Net.WebRequestMethods;
using Game.Scenes.Lobby;
using UnityEngine.TextCore.Text;
using Game.Network;
using Unity.VisualScripting;
using Client.Systems;

namespace MMG_CRPG.UI
{
    public class EquipItemWindowUI : MonoBehaviour
    {
        [Header("Item Detail")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text NameText;
        [SerializeField] private StatUI[] _itemStats;
        private Dictionary<string, StatUI> ItemStats = new();

        [Header("Item List")]
        [SerializeField] private BagItemIconUI prefab;
        [SerializeField] private RectTransform parents;

        [Header("Equip")]
        [SerializeField] private Button equipBtn;
        [SerializeField] private TMP_Text equipBtnText;
        private bool isEquip;
        private UserInventory currentInventoryItem;
        private UserInventory equipedInventoryItem;
        private EquipSlotPb slotData;


        private readonly List<BagItemIconUI> _pool = new();
        private readonly List<BagItemIconUI> _activeItems = new();

        private EquipType equipType;


        private void Awake()
        {
            ItemStats.Clear();
            ItemStats = _itemStats
                .ToDictionary(x => x.type.ToString(), x => x);

            equipBtn.onClick.AddListener(EquipItem);
        } 
        private void InitDetail(ItemMessage item = null, UserInventory inv = null)
        {
            iconImage.sprite = null;
            NameText.text = "";
            foreach (var itemStatScript in _itemStats)
                itemStatScript.gameObject.SetActive(false);
            currentInventoryItem = null;

            if (item != null && inv != null)
            {
                ShowDetail(item, inv);
                currentInventoryItem = inv;
            }
        }
        private List<UserInventory> GetTypeItem(EquipSlotPb slot)
        {
            var user = GameState.Instance.CurrentUser;
            var itemCache = ItemCache.Instance;
            var nowCharId = UserCharacterDeatailUI.Instance.status.CharacterId;

            int equipTypeId = itemCache.ItemTypeDictionary["EQUIP"].Id;
            var inventories = user.InventoryType[equipTypeId] ?? new List<UserInventory>();

            // 전체 장착 목록
            var allEquippedIds = EquipmentQuery.GetAllEquippedInventoryIds(user);

            // 현재 캐릭이 이 슬롯에 낀 인벤토리 ID
            long? currentInvId = EquipmentQuery.GetEquippedInventoryId(user, nowCharId, slot.Id);

            Debug.Log($"{nowCharId} {slot.Name} {slot.Id} 현재 장착 중인 Id " + (currentInvId ?? 0).ToString());
            if (currentInvId != null)
            {
                equipedInventoryItem = user.Inventory.Where(x => x.Value.Id == currentInvId).Select(x => x.Value).FirstOrDefault();
                if (equipedInventoryItem != null)
                    Debug.Log($"장착된 아이템이 있다. {equipedInventoryItem.Id}");
            }
            else
                equipedInventoryItem = null;

            bool IsOtherCharacterEquipped(UserInventory inv)
                => allEquippedIds.Contains(inv.Id) && inv.Id != currentInvId;

            //  정리된 필터링
            var filtered = inventories
                .Where(inv => EquipmentQuery.CheckSameSlot(inv, slot) && EquipmentQuery.IsEquippedByOthers(inv.Id, currentInvId, allEquippedIds) == false)
                .ToList();

            return EquipmentQuery.Filter_FirstEquippedId(currentInvId, filtered);
        }

        public void Set(EquipSlotPb equipSlotData)
        {
            slotData = equipSlotData;
            int typeNum = ItemCache.Instance.ItemTypeDictionary["EQUIP"].Id;
            // User 보유 Equip 타입 ItemId List
            UserData userData = GameState.Instance.CurrentUser;
            ItemCache itemCache = ItemCache.Instance;

            List<UserInventory> invenItems = GetTypeItem(equipSlotData);

            List<(ItemMessage, UserInventory)> EachTypeItem = invenItems    // 아이템 정보와 보유 갯수 
                .Where(x => itemCache.ItemDict[x.ItemId].EuqipType == equipSlotData.Id)
                .Select(x => (
                itemCache.ItemDict[x.ItemId],
                x
                ))
                .ToList(); 

            bool checkHaveItem = (EachTypeItem == null || EachTypeItem.Count == 0);
            InitDetail(checkHaveItem ? null : EachTypeItem[0].Item1, checkHaveItem ? null : EachTypeItem[0].Item2);


            foreach (Transform child in parents)
                child.gameObject.SetActive(false);

            foreach (var item in EachTypeItem)
            {
                var bagItemIconUI = GetOrCreateItemUI();
                bagItemIconUI.Set(item.Item1, item.Item2);
                bagItemIconUI.AddClickEvent(ShowDetail);
            }
        }
        private BagItemIconUI GetOrCreateItemUI()
        {
            var pooled = _pool.FirstOrDefault(i => !i.gameObject.activeSelf);
            if (pooled != null)
            {
                pooled.gameObject.SetActive(true);
                return pooled;
            }

            var instance = Instantiate(prefab, parents);
            _pool.Add(instance);
            return instance;
        }
        private void SetEquipButton(long invId)
        {
            if (equipedInventoryItem != null && invId == equipedInventoryItem.Id)
            {
                Debug.Log($"이미 장착된 놈 클릭한 아이템 {invId}, 장착된 아이템 {equipedInventoryItem.Id}");
                equipBtnText.text = "해제";
                isEquip = false;
            }
            else
            {
                Debug.Log($"이미 장착된 놈 클릭한 아이템 {invId}, ");
                isEquip = true;
                equipBtnText.text = "장착";
            }
        }

        public void ShowDetail(ItemMessage itemMessage, UserInventory inv)
        { 

            iconImage.sprite = MasterDataCache.Instance.IconSprites[itemMessage.IconId];
            NameText.text = itemMessage.Name;

            currentInventoryItem = inv;
            SetEquipButton(inv.Id);

            foreach (var itemStatScript in _itemStats)
                itemStatScript.gameObject.SetActive(false);
            foreach (var stat in itemMessage.Stats)
            {
                ItemStats[stat.Code].gameObject.SetActive(true);
                ItemStats[stat.Code].SetValue((int)stat.Value);
            }
        }
        public void EquipItem() // 아이템 장착 
        {
            var req = new SetEquipmentRequest
            {
                EquipId = slotData.Id,
                InventoryId = currentInventoryItem.Id
            };
            if(isEquip == false)
            {
                req = new SetEquipmentRequest
                {
                    EquipId = slotData.Id,
                };
            }


            UserCharacterDeatailUI detailUI = UserCharacterDeatailUI.Instance;
            string path = ApiRoutes.UserCharacterEquip(GameState.Instance.CurrentUser.UserId, detailUI.EquipUI.currentCharacter.Id, slotData.Id);
            Debug.Log($"장착 : {path}");
            StartCoroutine(AppBootstrap.Instance.Http.Put(path, req, SetEquipmentResponse.Parser, OnEquipResponse));
        } 

        private void OnEquipResponse(ApiResult<SetEquipmentResponse> result)
        {
            if (!result.Ok)
            {
                Debug.LogError("장착 실패: " + result.Message);
                return;
            }

            var res = result.Data;
            Debug.Log($"장착 성공! equip={res.Slot.EquipId}, inv={res.Slot.InventoryId}");
            var user = GameState.Instance.CurrentUser;
            var itemCache = ItemCache.Instance;
            var nowCharId = UserCharacterDeatailUI.Instance.status.CharacterId;
            long? currentInvId = EquipmentQuery.GetEquippedInventoryId(user, nowCharId, slotData.Id); 
            equipedInventoryItem = GameState.Instance.CurrentUser.Inventory.Select(x => x.Value).Where(x => x.Id == currentInvId).FirstOrDefault();
            if (res.Slot.InventoryId == 0)
                equipedInventoryItem = null;
            Set(slotData);  

            RefreshSlotUI(res);
        }
        private void RefreshSlotUI(SetEquipmentResponse res)
        {
            UserData user = GameState.Instance.CurrentUser;

            // 캐릭터에 아이템 최신화 
            user.ApplyEquipmentSnapshot(res);
            // UI 반영하기 : UserCharacterEquipUI에 해당 아이템으로 바꾸기 
            UserCharacterDeatailUI.Instance.EquipUI.RefreshEquipIcon();
            this.gameObject.SetActive(false);
        }

    }
}