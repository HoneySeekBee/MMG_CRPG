using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Contracts.EquipSlots;
using Game.Data;
using Contracts.Protos;

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

        private readonly List<BagItemIconUI> _pool = new();
        private readonly List<BagItemIconUI> _activeItems = new();

        private EquipType equipType;


        private void Start()
        {
            ItemStats.Clear();
            ItemStats = _itemStats
                .ToDictionary(x => x.type.ToString(), x => x);
        }
        private void InitDetail()
        {
            iconImage.sprite = null;
            NameText.text = "";
            foreach (var itemStatScript in _itemStats)
                itemStatScript.gameObject.SetActive(false);
        }
        private List<UserInventory> GetTypeItem(EquipSlotPb slot)
        {
            var user = GameState.Instance.CurrentUser;
            var itemCache = ItemCache.Instance;
            var nowCharId = UserCharacterDeatailUI.Instance.status.CharacterId;

            int equipTypeId = itemCache.ItemTypeDictionary["EQUIP"].Id;
            var inventories = user.InventoryType[equipTypeId] ?? new List<UserInventory>();

            // 전체 장착 목록
            var allEquippedIds = user.UserCharactersDict.Values
                .SelectMany(c => c.Equips ?? Enumerable.Empty<UserCharacterEquipPb>())
                .Select(e => e.InventoryId)
                .Distinct()
                .ToHashSet();

            // 현재 캐릭이 이 슬롯에 낀 인벤토리 ID
            long? currentInvId = user.UserCharactersDict.Values
                .FirstOrDefault(c => c.CharacterId == nowCharId)?
                .Equips?.FirstOrDefault(e => e.EquipId == slot.Id)?
                .InventoryId;

            bool IsSameSlotType(UserInventory inv)
                => itemCache.ItemDict[(long)inv.ItemId].EuqipType == slot.Id;

            bool IsOtherCharacterEquipped(UserInventory inv)
                => allEquippedIds.Contains(inv.Id) && inv.Id != currentInvId;

            //  정리된 필터링
            var filtered = inventories
                .Where(inv => IsSameSlotType(inv) && !IsOtherCharacterEquipped(inv))
                .ToList();

            //  현재 장착한 장비 맨 앞에
            if (currentInvId is long cid)
            {
                var idx = filtered.FindIndex(inv => inv.Id == cid);
                if (idx > 0)
                {
                    var cur = filtered[idx];
                    filtered.RemoveAt(idx);
                    filtered.Insert(0, cur);
                }
            }

            return filtered;
        }

        public void Set(EquipSlotPb equipSlotData)
        {
            int typeNum = ItemCache.Instance.ItemTypeDictionary["EQUIP"].Id;
            // User 보유 Equip 타입 ItemId List
            UserData userData = GameState.Instance.CurrentUser; 
            ItemCache itemCache = ItemCache.Instance;

            List<UserInventory> invenItems = GetTypeItem(equipSlotData);
            

            List<(ItemMessage, int)> EachTypeItem = invenItems    // 아이템 정보와 보유 갯수 
                .Where(x => itemCache.ItemDict[x.ItemId].EuqipType == equipSlotData.Id)
                .Select(x => (
                itemCache.ItemDict[x.ItemId],
                x.Count
                ))
                .ToList();


            InitDetail();

            foreach (var item in userData.Inventory)
            {
                Debug.Log($"아이템 {item.Key} 타입 {itemCache.ItemDict[(long)item.Key].EuqipType} || Id {item.Value.Id}");
            }

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
        public void ShowDetail(ItemMessage itemMessage)
        {
            iconImage.sprite = MasterDataCache.Instance.IconSprites[itemMessage.IconId];
            NameText.text = itemMessage.Name;

            foreach (var itemStatScript in _itemStats)
                itemStatScript.gameObject.SetActive(false);
            foreach (var stat in itemMessage.Stats)
            {
                Debug.Log($"{stat.Code} ");
                ItemStats[stat.Code].gameObject.SetActive(true);
                ItemStats[stat.Code].SetValue((int)stat.Value);
            }
        }
    }

}