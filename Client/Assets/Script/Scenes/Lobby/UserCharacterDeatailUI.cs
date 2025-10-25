using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserCharacterDeatailUI : MonoBehaviour
{
    public static UserCharacterDeatailUI Instance { get; private set; }

    [SerializeField] private UserCharacterStatusUI StatusUI;
    public UserCharacterSummaryPb status;
    [SerializeField] private Toggle[] CategoryToggles;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void Set(UserCharacterSummaryPb status)
    {
        this.status = status;
        CategoryToggles[0].isOn = true;
        StatusUI.Set();
    } 
}
