using Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static LoginPopup;

public class BattleLobbyPopup : UIPopup
{
    [SerializeField] private Button AdventureBtn; 

    public void Set_AdventureBtn(Action onAdventureClicked)
    {
        AdventureBtn.onClick.AddListener(() =>
        {
            onAdventureClicked?.Invoke();
        });
    }
}
