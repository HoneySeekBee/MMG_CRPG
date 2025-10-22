using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserCharacterDeatailUI : MonoBehaviour
{
    [SerializeField] private UserCharacterStatusUI StatusUI;

    public void Set(UserCharacterSummaryPb status)
    {
        StatusUI.Set(status);
    }


}
