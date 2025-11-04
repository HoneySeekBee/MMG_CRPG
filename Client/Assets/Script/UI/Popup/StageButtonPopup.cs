using Lobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StageButtonPopup : UIPopup
{
    [Header("UI Obj")]
    [SerializeField] private TMP_Text ChapterStageText; 
    [SerializeField] private Button StageButton;
    [SerializeField] private Image StageButtonImage;
    [SerializeField] private Image[] StageStars;
    [SerializeField] private GameObject StarObj;
    [SerializeField] private GameObject LockObj;
     
    public void Set(int chapterNum, int stageNum, Action onStageClicked, bool isActive, int score = 0)
    {
        ChapterStageText.text = $"{chapterNum}-{stageNum}";
        StageButton.onClick.AddListener(() =>
        {
            onStageClicked?.Invoke();
        });
        Sprite yellowStar = UIImageCache.Instance.Get(UIImageCache.YellowStarKey);
        Sprite grayStar = UIImageCache.Instance.Get(UIImageCache.GrayStarKey);
        if(isActive)
        {
            for(int i = 0; i < StageStars.Length; i++)
            {
                StageStars[i].sprite = (i <= score - 1) ? yellowStar : grayStar;
            }
        }
        StageButtonImage.sprite = isActive ? UIImageCache.Instance.Get(UIImageCache.ButtonGreenKey) : UIImageCache.Instance.Get(UIImageCache.ButtonGreenKey);
        StarObj.SetActive(isActive);
        LockObj.SetActive(!isActive);
    }
}
