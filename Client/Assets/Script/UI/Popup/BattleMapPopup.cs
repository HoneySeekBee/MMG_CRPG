using Contracts.Protos;
using Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleMapPopup : UIPopup
{
    public static BattleMapPopup Instance { get; private set; }
    [Header("UI")]
    [SerializeField] private Button PauseBtn;
    [SerializeField] private Button SpeedBtn;
    [SerializeField] private Button AutoBtn;
    [SerializeField] private TMP_Text TimeText;
    [SerializeField] private Transform SkillIconTr;

    [SerializeField] private GameObject StartPopup;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    // 이제 여기서 스테이지에 대한 정보를 받아야한다. 
    public void Set(Action fadeIn)
    {
        StartCoroutine(BattleMapManager.Instance.Set_BattleMap(fadeIn));
        StartPopup.SetActive(false);
    }
    
    public IEnumerator ShowStart()
    {
        StartPopup.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        StartPopup.SetActive(false);
    }
}
