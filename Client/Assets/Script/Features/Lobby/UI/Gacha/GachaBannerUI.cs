using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Contracts.Protos;
using System;

public class GachaBannerUI : MonoBehaviour
{
    private GachaBannerPb Data; 
    [SerializeField] private Toggle bannerToggle;
    [SerializeField] private TMP_Text nameText;
    private Sprite GachaPortraits; // 나중에 쓸것이다. 

    public void Set(Action<GachaBannerPb, Sprite> action, GachaBannerPb _data, ToggleGroup toggleGroup)
    { 
        Data = _data;
        bannerToggle.group = toggleGroup;
        nameText.text = Data.Title;
        GachaPortraits = MasterDataCache.Instance.PortraitSprites[Data.PortraitId];

        bannerToggle.onValueChanged.AddListener((isOn =>
        {
            if (isOn)
                action?.Invoke(Data, GachaPortraits);
        }));

    }
}
