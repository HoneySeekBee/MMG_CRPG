using Lobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventureLobbyPopup : UIPopup
{
    [Header("UI")]
    [SerializeField] private Dropdown ChapterTitles;
    [SerializeField] private Transform Row1;
    [SerializeField] private Transform Row2;

    public void GetData()
    {
        // 유저의 스테이지 정보에 대해서 가지고 와야한다. 
    }
}
