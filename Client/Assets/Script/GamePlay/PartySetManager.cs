using Contracts.UserParty;
using Game.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartySetManager : MonoBehaviour
{
    public static PartySetManager Instance { get; private set; }
    [System.Serializable]
    private class PartySlot
    {
        public int slotNum;
        public BatchSlot batchSlot;
    }

    [SerializeField] private PartySlot[] partySlots;
    [HideInInspector] public Dictionary<int, BatchSlot> partySlotsDict = new();

    public const int MAX_CHARACTER_COUNT = 6; 
    [Header("캐릭터 오브젝트 ")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform poolParent;
    [HideInInspector] public List<GameObject> character_pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // [1] 이전에 배치한 정보를 불러온다. 
    public void Initialize(int battleId, Action refresh)
    {
        ClearAll();
        partySlotsDict = partySlots.ToDictionary(p => p.slotNum, p => p.batchSlot);
        foreach (PartySlot p in partySlots)
        {
            p.batchSlot.Set(p.slotNum, refresh);
        }

        List<UserPartySlotPb> slots = GameState.Instance.CurrentUser.UserPartyList[battleId];
        foreach (UserPartySlotPb slot in slots)
        {
            if (slot == null) continue;
            if(slot.SlotId == 0) continue;
            if (slot.UserCharacterId != null)
            {
                Debug.Log($"{slot}번 슬롯 : {slot.UserCharacterId}");
                partySlotsDict[slot.SlotId].BatchCharacter(slot.UserCharacterId?? 0, GetCharacterObject());
            }
            partySlotsDict[slot.SlotId].SetData(slot);
        }
    }
    public int AssignedCount()
    {
        int assignedCount = partySlotsDict.Values
    .Count(slot => slot != null
                && slot.SlotData != null
                && slot.SlotData.UserCharacterId > 0);
        return assignedCount;
    }
    // [2] 장착하기 
    public bool BatchCharacter(int formationNum, int characterNum)
    {
        bool check = false;
        int assignedCount = AssignedCount();
        if (assignedCount > MAX_CHARACTER_COUNT)
        {
            Debug.Log("배치 할 수 있는 캐릭터가 가득찼어요.");
            return false;
        }

        BatchSlot batch = null;

        for (int i = formationNum * 3; i < formationNum * 3 + 3; i++)
        {
            if(partySlotsDict[i].CheckEmpty())
            {
                check = true;
                batch = partySlotsDict[i];
                break;
            }
        }

        if(check == false)
        { 
            return false;
        }

        batch.BatchCharacter(characterNum, GetCharacterObject());
        return true;
    }

    public GameObject GetCharacterObject()
    {
        var pooled = character_pools.Find(obj => !obj.activeSelf);
        if (pooled != null)
        {
            pooled.SetActive(true);
            return pooled;
        }

        // 없으면 새로 생성
        var newObj = Instantiate(prefab , poolParent);
        character_pools.Add(newObj);
        return newObj;
    }
    public void ReturnCharacter(GameObject character)
    {
        if (character == null) return;
        character.SetActive(false);
        character.transform.position = poolParent.transform.position;
    }
    public void ClearAll()
    {
        foreach (var obj in character_pools)
        {
            if (obj != null)
                Destroy(obj);
        }
        character_pools.Clear();
    }
    public void SaveCurrentBattleParty(Action<bool> onDone = null)
    {
        var battleId = LobbyRootController.Instance._currentBattleId; 
        var partyId = GameState.Instance.CurrentUser.UserPartyIdByBattleId[battleId]; 
        SaveCurrentParty(partyId, onDone);
    }
    public void SaveCurrentParty(long partyId, Action<bool> onDone = null)
    {
        // partySlots → (slotId, userCharacterId) 로 변환
        var pairs = partySlots.Select(ps =>
        {
            // batchSlot이나 SlotData가 비어있을 수도 있으니 null 체크
            int? userCharacterId = ps.batchSlot?.SlotData != null
                ? (int?)ps.batchSlot.SlotData.UserCharacterId
                : null;
            Debug.Log($"[파티 저장] - {userCharacterId}");
            // 캐릭터 아이디 0? => null 
            if (userCharacterId == 0)
                userCharacterId = null;

            return (ps.slotNum, userCharacterId);
        });

        NetworkManager.Instance.PartyNetwork.SaveParty(partyId, pairs, onDone);
    }
}
