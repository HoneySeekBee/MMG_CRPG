using Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatSpeedBtn : MonoBehaviour
{
    private Button btn;
    [SerializeField] private Image speedImage;
    [SerializeField] private Sprite[] speedImagePool;

    public void Set(Action buttonAction)
    {
        btn = this.GetComponent<Button>();

        // 스테이지가 시작된다? 기본값 
        speedImage.sprite = speedImagePool[0];
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => buttonAction.Invoke());
    }
    public void UpdateSpeedUI(CombatSpeedPb speed)
    {
        int spriteNum = speed == CombatSpeedPb.CombatSpeedX1 ? 0 : speed == CombatSpeedPb.CombatSpeedX15 ? 1 : 2;
        speedImage.sprite = speedImagePool[spriteNum];
    }

}
