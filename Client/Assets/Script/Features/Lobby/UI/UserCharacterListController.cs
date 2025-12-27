using Contracts.Protos;
using Game.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserCharacterListController : MonoBehaviour
{
    public event Action<List<UserCharacterSummaryPb>> OnListChanged;

    private List<UserCharacterSummaryPb> _userCharacters = new();

    public void RefreshList()
    {
        _userCharacters = GameState.Instance.CurrentUser.GetAllUserCharacters();
        OnListChanged?.Invoke(_userCharacters);
    }
}
