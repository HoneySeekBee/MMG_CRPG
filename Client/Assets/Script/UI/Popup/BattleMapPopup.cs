using Contracts.Protos;
using Lobby;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleMapPopup : UIPopup
{
    private StagePb stageData;

    [Header("UI")]
    [SerializeField] private Button PauseBtn;
    [SerializeField] private Button SpeedBtn;
    [SerializeField] private Button AutoBtn;
    [SerializeField] private TMP_Text TimeText;
    [SerializeField] private Transform SkillIconTr;

    // 이제 여기서 스테이지에 대한 정보를 받아야한다. 
    public async void Set()
    {
        stageData = LobbyRootController.Instance._currentStage;

        foreach(var batch in stageData.Batches)
        { 
            var UnitObj = await AddressableManager.Instance.LoadAsync<GameObject>(batch.UnitKey);
            var EnvObj = await AddressableManager.Instance.LoadAsync<GameObject>(batch.EnvKey);
            GameObject unit = Instantiate(UnitObj);
            GameObject env = Instantiate(EnvObj, unit.transform);
            unit.transform.position = new Vector3(20 * (batch.BatchNum - 1), 0, 0);
        }
    }


}
