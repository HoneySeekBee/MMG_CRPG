using Contracts.Protos;
using Contracts.UserParty;
using Game.Data;
using Game.Managers;
using Lobby;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using WebServer.Protos;

public class PartySetupPopup : UIPopup
{
    [Header("캐릭터 리스트 UI")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform contents;
    private readonly List<UserCharacterUI> uiPool = new();

    private List<UserCharacterSummaryPb> UserCharacters = new();

    private void Set_Character()
    {
        UserCharacters = GameState.Instance.CurrentUser.UserCharactersDict
        .Select(x => x.Value)
        .ToList(); 
    }
    public void Set()
    {
        StartCoroutine(LoadPartySet());
        Set_Character();
       
    }
    private IEnumerator LoadPartySet()
    {
        yield return SceneController.Instance.LoadAdditiveAsync(SceneController.PartySetupSceneName);
        PartySetManager.Instance.Initialize(BattleLobbyPopup.BATTLE_ADVENTURE, Refresh_CharacterList);
        Refresh_CharacterList();
    }
    private void Refresh_CharacterList()
    {
        List<UserCharacterSummaryPb> _characters = GetNoBatchCharacters();
        foreach (var ui in uiPool) { ui.gameObject.SetActive(false); }
        if (_characters == null || _characters.Count == 0)
            return;

        for (int i = 0; i < _characters.Count; i++)
        {
            UserCharacterUI ui;
            if (i < uiPool.Count)
            {
                ui = uiPool[i];
            }
            else
            {
                ui = Instantiate(prefab, contents).GetComponent<UserCharacterUI>();
                uiPool.Add(ui);
            }

            var summary = _characters[i];
            ui.Set(summary, (data) =>
            {
                CharacterDetailPb cha = CharacterCache.Instance.DetailById[summary.CharacterId];
                Debug.Log($"[{cha.Name}을 {cha.FormationNum}에 배치합니다. ");
                Batch(cha.FormationNum, cha.Id);
            });
            ui.gameObject.SetActive(true);
        }
    }

    private void Batch(int formationNum, int characterNum)
    {
        // formationNum [ 1 : 전위 / 2 : 중위 / 3 : 후위 ]  
        PartySetManager partyManager = PartySetManager.Instance;
        // (1) 각 Formation 별로 배치할 수 있는 슬롯이 있는지 파악하기 

        if(partyManager.BatchCharacter(formationNum, characterNum) == false)
        {
            Debug.Log("자리가 없습니다. ");
        }
        else
        {
            Debug.Log($"배치하기 {formationNum}, {characterNum} ");
            Refresh_CharacterList();
        }

        // (2) 
    }
    // [1] 유저 보유 캐릭터 불러오기
    private List<UserCharacterSummaryPb> GetNoBatchCharacters()
    {
        var assignedIds = PartySetManager.Instance.partySlotsDict.Values
       .Select(slot => slot != null ? slot.GetSlotData() : null)   // BatchSlot → UserPartySlotPb
       .Where(slotData => slotData != null && slotData.UserCharacterId > 0)
       .Select(slotData => slotData.UserCharacterId)
       .ToHashSet();

        return UserCharacters
            .Where(ch => !assignedIds.Contains(ch.CharacterId))
            .ToList();
    }
    // [2] 유저의 파티 정보를 불러와야 한다. 

    // [3] 파티 구성 씬을 불러와야한다. 



}
