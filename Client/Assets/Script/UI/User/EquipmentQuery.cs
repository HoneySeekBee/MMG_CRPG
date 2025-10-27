using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Contracts.Protos;
using Contracts.EquipSlots;
using OpenCover.Framework.Model;

namespace MMG_CRPG.UI
{
    public class EquipmentQuery
    {
        // 현재 선택된 캐릭터가 특정 부위의 아이템을 장착했는지 번호 
        public static long? GetEquippedInventoryId(UserData user, long nowCharId, long slotId)
        {
            if (user == null) return null;

            // 캐릭터 접근: LINQ FirstOrDefault 대신 딕셔너리/인덱싱이 있으면 그걸 권장
            var character = user.UserCharactersDict.Values
                .FirstOrDefault(c => c.CharacterId == nowCharId);

            if (character?.Equips == null) return null;

            var equip = character.Equips.FirstOrDefault(e => e.EquipId == slotId);
            return equip?.InventoryId;
        }

        public static HashSet<long> GetAllEquippedInventoryIds(UserData user)
        {
            if (user?.UserCharactersDict == null)
                return new HashSet<long>();

            return user.UserCharactersDict.Values
                .SelectMany(c => c?.Equips ?? Enumerable.Empty<UserCharacterEquipPb>())
                .Select(e => e.InventoryId)
                .Where(id => id != 0)              
                .ToHashSet();
        }

        public static bool CheckSameSlot(UserInventory inv, EquipSlotPb slot)
        {
            var itemCache = ItemCache.Instance;
            return itemCache.ItemDict[(long)inv.ItemId].EuqipType == slot.Id;
        }
         

        // 장착 중인 장비 맨앞에 오게 하기 
        public static List<UserInventory> Filter_FirstEquippedId(long? currentInvId, List<UserInventory> filtered)
        {
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
        public static List<UserInventory> GetUserEquipInventories(UserData user)
        {
            var itemCache = ItemCache.Instance;
            int equipTypeId = itemCache.ItemTypeDictionary["EQUIP"].Id;
            // 없으면 빈 리스트 반환 (null 방어)
            return user?.InventoryType != null && user.InventoryType.TryGetValue(equipTypeId, out var list) && list != null
                ? list
                : new List<UserInventory>();
        }
        public static bool IsEquippedByOthers(long invId, long? currentInvId, HashSet<long> allEquippedIds)
            => allEquippedIds.Contains(invId) && invId != (currentInvId ?? -1);
        public static int GetIconIdForSlotOrDefault(UserData user, long nowCharId, EquipSlotPb slot)
        {
            var itemCache = ItemCache.Instance;
            var invList = GetUserEquipInventories(user);
            var currentInvId = GetEquippedInventoryId(user, nowCharId, slot.Id);
            if (currentInvId == null) return slot.IconId;

            // InventoryType[equip] 리스트에서 InventoryId로 검색
            var inv = invList.FirstOrDefault(v => v.Id == currentInvId.Value);
            if (inv == null) return slot.IconId;

            return itemCache.ItemDict[(long)inv.ItemId].IconId;
        }
    }
}