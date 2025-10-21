using Contracts.Protos;
using Game.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserCharactersListUI : MonoBehaviour
{
    public static UserCharactersListUI Instance { get; private set; }

    public RectTransform IconRectTransform;
    public UserCharacterUI UserCharacterPrefab;

    [HideInInspector] public List<UserCharacterUI> UserCharacterUIList = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void OnEnable()
    {
        Set();
    }

    private void Set()
    {
        List<UserCharacterSummaryPb> userCharacterList = GameState.Instance.CurrentUser.GetAllUserCharacters();

        foreach (var userCharacter in userCharacterList)
        {
            UserCharacterUI eachCharacter = Instantiate(UserCharacterPrefab.gameObject).GetComponent<UserCharacterUI>();
            UserCharacterUIList.Add(eachCharacter);
            eachCharacter.Set(userCharacter);
        }
    }
}
