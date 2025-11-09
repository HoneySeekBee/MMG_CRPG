using Contracts.UserParty;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatchSlot : MonoBehaviour
{
    [HideInInspector] public UserPartySlotPb SlotData;
    private int SlotNum;
    private Action onRefresh;
    private GameObject CharacterObject;
    public void Set(int slotNum, Action refresh)
    {
        SlotNum = slotNum;
        this.onRefresh = refresh;
    }
    public void SetData(UserPartySlotPb slotdata)
    {
        SlotData = slotdata;
    }
    public UserPartySlotPb GetSlotData()
    {
        return SlotData;
    }
    private void OnMouseDown()
    {
        if (SlotData == null)
        {
            Debug.Log($"[{name}] 슬롯 데이터가 없음");
            return;
        }

        Debug.Log(
            $"[{name}] 슬롯 클릭! slotId={SlotData.SlotId}, userCharId={SlotData.UserCharacterId ?? 0}"
        );
        Unassign();
    }
    public bool CheckEmpty()
    {
        return SlotData == null || SlotData.UserCharacterId == 0;
    }
    public void BatchCharacter(int characterId, GameObject characterObject)
    {
        Debug.Log($"{SlotNum}번에 캐릭터 {characterId}가 배치되었습니다.");
        if (SlotData == null)
        {
            SlotData = new UserPartySlotPb();
            SlotData.UserCharacterId = characterId;
            SlotData.SlotId = SlotNum;
        }
        else
            SlotData.UserCharacterId = characterId;
        CharacterObject = characterObject;
        CharacterObject.transform.parent = this.transform;
        CharacterObject.transform.position = this.transform.position;
        CharacterAppearance appearance = CharacterObject.GetComponent<CharacterAppearance>();
        appearance.Set(CharacterCache.Instance.CharacterModelById[characterId]);
    }
    private void Unassign()
    {
        if (SlotData.UserCharacterId == 0)
            return;
        SlotData.UserCharacterId = 0;
        onRefresh?.Invoke();
        PartySetManager.Instance.ReturnCharacter(CharacterObject);
    }
}
