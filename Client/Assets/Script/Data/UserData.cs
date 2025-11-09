
using Contracts.Protos;
using Contracts.UserParty;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserData
{
    // 기본 정보
    public int UserId { get; private set; }
    public string Nickname { get; private set; }
    public int Level { get; private set; }

    // 재화
    public int SoftCurrency { get; private set; }
    public int HardCurrency { get; private set; }

    public UserProfilePb UserProfilePb { get; private set; }

    // 인벤토리
    private readonly Dictionary<long, UserInventory> _inventory = new(); // ItemId -> UserInventory
    public Dictionary<long, UserInventory> Inventory => _inventory;

    private readonly Dictionary<int, List<UserInventory>> _inventoryType = new(); // 각 타입 별 인벤토리 Id 
    public IReadOnlyDictionary<int, List<UserInventory>> InventoryType => _inventoryType;

    // 보유 캐릭터 
    private readonly Dictionary<int, UserCharacterSummaryPb> _userCharactersDict = new();

    public Dictionary<int, UserCharacterSummaryPb> UserCharactersDict
        => _userCharactersDict.ToDictionary(kv => kv.Key, kv => kv.Value.Clone()); // 외부에 Clone

    public StageProgressManager StageProgress { get; } = new StageProgressManager();

    // 파티 
    private readonly Dictionary<int, List<UserPartySlotPb>> _userpartyList = new(); // 각 BattleType별 파티 구성 
    public IReadOnlyDictionary<int, List<UserPartySlotPb>> UserPartyList => _userpartyList;
    public GetUserPartyResponsePb PartyProgress { get; } = new GetUserPartyResponsePb();


    public UserData(int userId, string nickname, int level)
    {
        UserId = userId;
        Nickname = nickname;
        Level = level;
    }
    public void SyncCurrencies(int soft, int hard)
    {
        SoftCurrency = soft;
        HardCurrency = hard;
    }
    public void SyncInventory(IEnumerable<UserInventory> items)
    {
        _inventory.Clear();
        _inventoryType.Clear();

        foreach (var i in items)
        {
            _inventory[i.Id] = i;
            
            int TypeNum = ItemCache.Instance.ItemDict[i.ItemId].TypeId;

            if (!_inventoryType.ContainsKey(TypeNum))
                _inventoryType[TypeNum] = new List<UserInventory>();
            _inventoryType[TypeNum].Add(i);
            Debug.Log($"유저의 인벤토리 아이템 카테고리 {TypeNum} : 아이디 {i.ItemId} : 갯수 {_inventory.Count}");
        }

    } 
    public int GetItemCount(int itemId)
            => _inventory.TryGetValue(itemId, out var cnt) ? cnt.Count : 0;

    public void SyncCharacters(IEnumerable<UserCharacterSummaryPb> userCharacters)
    { 
        var incoming = new Dictionary<int, UserCharacterSummaryPb>();

        Debug.Log($"1 [UserCharacters] {userCharacters.Count()}");

        foreach (var uc in userCharacters)
            incoming[uc.CharacterId] = uc; 
        
        // 추가/업데이트: UpdatedAt이 더 최신인 경우만 덮어쓰기
        foreach (var (charId, inc) in incoming)
        {
            if (_userCharactersDict.TryGetValue(charId, out var cur))
            {
                if (ToDto(inc.UpdatedAt) > ToDto(cur.UpdatedAt))
                    _userCharactersDict[charId] = inc.Clone();
            }
            else
            {
                _userCharactersDict[charId] = inc.Clone();
            }
        }

        Debug.Log($"2 [UserCharacters] {incoming.Count()}");
    }
    public void ApplyEquipmentSnapshot(SetEquipmentResponse res)
    {
        if (!UserCharactersDict.TryGetValue(res.CharacterId, out var ch))
        {
            Debug.LogError($"Character {res.CharacterId} not found in UserData");
            return;
        }

        // Clone일 가능성이 있으므로 새로운 객체 생성
        var updated = ch.Clone();

        // 기존 equips 다 비우고, 서버 스냅샷으로 덮어씌움
        updated.Equips.Clear();
        updated.Equips.AddRange(res.Equips);

        // dict에 다시 반영 (기억: 기존 프로퍼티는 Clone을 반환하므로 set은 직접 해야 함)
        _userCharactersDict[res.CharacterId] = updated;
    }

    public bool AddOrUpdateCharacter(UserCharacterSummaryPb character)
    {
        var inc = character;
        if (_userCharactersDict.TryGetValue(inc.CharacterId, out var cur))
        {
            if (ToDto(inc.UpdatedAt) <= ToDto(cur.UpdatedAt))
                return false; // 구버전이면 무시
        }
        _userCharactersDict[inc.CharacterId] = inc.Clone();
        return true;
    }

   public void SyncUserParty(int partyid, IEnumerable<UserPartySlotPb> userPartys)
    {
        _userpartyList[partyid] = new List<UserPartySlotPb>();
        foreach (var userPartySlot in userPartys)
        {
            _userpartyList[partyid].Add(userPartySlot);
        }
    }

    public bool RemoveCharacter(int characterId) => _userCharactersDict.Remove(characterId);

    // 스킬 업서트 (필요 시 스킬 딕셔너리 캐시 추가 고려)
    public bool UpsertSkill(int characterId, UserCharacterSkillPb skill)
    {
        if (!_userCharactersDict.TryGetValue(characterId, out var ch)) return false;

        var list = ch.Skills;
        var idx = list.ToList().FindIndex(s => s.SkillId == skill.SkillId);

        if (idx >= 0)
        {
            // 최신 것만 반영
            var cur = list[idx];
            if (ToDto(skill.UpdatedAt) <= ToDto(cur.UpdatedAt)) return false;

            list[idx] = skill.Clone();
        }
        else
        {
            list.Add(skill.Clone());
        }

        // 캐릭터 UpdatedAt도 갱신(정책에 맞게)
        ch.UpdatedAt = skill.UpdatedAt;
        return true;
    }

    public List<UserCharacterSummaryPb> GetAllUserCharacters()
    => _userCharactersDict.Values
                         .Select(x => x.Clone())
                         .ToList();
    public UserCharacterSummaryPb TryGetCharacter(int characterId)
        => _userCharactersDict.TryGetValue(characterId, out var ch) ? ch.Clone() : null; 
     
    public void SetUserProfile(UserProfilePb _userProfile)
    {
        UserProfilePb = _userProfile;
        Debug.Log($"[SetUserProfile] {UserProfilePb == null}");
    }
    private static DateTimeOffset ToDto(Timestamp ts)
        => ts == null ? DateTimeOffset.MinValue : ts.ToDateTimeOffset();

    public void SyncStageProgress(MyStageProgressListPb pb)
    { 
        StageProgress.Sync(pb);
    }
    public bool TryGetStageProgress(int stageId, out UserStageProgressPb progress)
    {
        var p = StageProgress.GetProgress(stageId);
        progress = p;
        return p != null;
    }

    public int GetStars(int stageId)
    {
        var p = StageProgress.GetProgress(stageId);
        return p == null ? 0 : (int)p.Stars;
    }

    public bool IsStageCleared(int stageId)
    {
        var p = StageProgress.GetProgress(stageId);
        return p != null && p.Cleared;
    }
}
