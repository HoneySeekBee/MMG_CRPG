using Contracts.Protos;
using Contracts.UserParty;
using Game.Data;
using Game.Managers;
using Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WebServer.Protos;

public class PartySetupPopup : UIPopup
{
    [Header("캐릭터 리스트 UI")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform contents;
    private readonly List<UserCharacterUI> uiPool = new();
    [SerializeField] private Button startBtn;
    private List<UserCharacterSummaryPb> UserCharacters = new();

    private void Set_Character()
    {
        UserCharacters = GameState.Instance.CurrentUser.UserCharactersDict
        .Select(x => x.Value)
        .ToList();
    }
    public void Set(Action action)
    {
        StartCoroutine(LoadPartySet(action));
        Set_Character();
        startBtn.onClick.RemoveAllListeners();
        startBtn.onClick.AddListener(GameStart);
    }
    private IEnumerator LoadPartySet(Action action = null)
    {
        yield return CharacterCache.Instance.CoPreloadMeshes(); 
        yield return SceneController.Instance.LoadAdditiveAsync(SceneController.PartySetupSceneName);
        PartySetManager.Instance.Initialize(NetworkManager.BATTLE_ADVENTURE, Refresh_CharacterList);
        Refresh_CharacterList();
        if (action != null)
            action.Invoke();
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

        if (partyManager.BatchCharacter(formationNum, characterNum) == false)
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

    private void GameStart()
    {
        PartySetManager partyManager = PartySetManager.Instance;
        int assignedCount = partyManager.AssignedCount();
        if (assignedCount == 0) // 파티에 배치된 캐릭터가 0
        {
            Debug.Log("캐릭터를 배치하세요");
            return;
        }
        if (assignedCount > PartySetManager.MAX_CHARACTER_COUNT) // 파티 인원 수 초과 
        {
            Debug.Log("파티 최대 멤버 수를 초과하였습니다. ");
            return;
        }

        Debug.Log("스테이지를 실행합니다. ");
        StartCoroutine(StartStageLogic());
    }
    private IEnumerator StartStageLogic()
    {
        startBtn.onClick.RemoveAllListeners();
        // 1) 파티 저장 기다리기
        bool isDone = false;
        bool isSuccess = false;

        PartySetManager.Instance.SaveCurrentBattleParty(success =>
        {
            isSuccess = success;
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);

        if (!isSuccess)
        {
            Debug.LogError($"[{LobbyRootController.Instance._currentBattleId}]의 파티 저장을 실패하였습니다.");
            startBtn.onClick.AddListener(GameStart);
            yield break;
        }

        // 2) 씬 언로드
        yield return SceneController.Instance.UnloadAdditiveAsync(SceneController.PartySetupSceneName);
        // 3) 씬 로드
        yield return SceneController.Instance.LoadAdditiveAsync(SceneController.MapSceneName);

        LobbyRootController.Instance.Show("BattleMap");
    }
}
