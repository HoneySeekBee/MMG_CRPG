using Contracts.Protos;
using Lobby;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleMapPopup : UIPopup
{  
    [Header("UI")]
    [SerializeField] private Button PauseBtn;
    [SerializeField] private Button SpeedBtn;
    [SerializeField] private Button AutoBtn;
    [SerializeField] private TMP_Text TimeText;
    [SerializeField] private Transform SkillIconTr;

    // 이제 여기서 스테이지에 대한 정보를 받아야한다. 
    public void Set()
    {
        StartCoroutine(BattleMapManager.Instance.Set_BattleMap());
    } 
}
