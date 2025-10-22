using Contracts.Protos;
using Game.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserCharactersListUI : MonoBehaviour
{
    public static UserCharactersListUI Instance { get; private set; }

    public RectTransform iconParent;
    public UserCharacterUI prefab;
    [SerializeField] private UserCharacterListController controller;

    private readonly List<UserCharacterUI> uiPool = new();

    public UserCharacterDeatailUI UserCharacterDeatailScript;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        controller.OnListChanged += UpdateUI;
    }
    private void OnEnable()
    {
        controller.RefreshList();
    }

    private void UpdateUI(List<UserCharacterSummaryPb> characters)
    {
        foreach (var ui in uiPool)
            ui.gameObject.SetActive(false);

        for (int i = 0; i < characters.Count; i++)
        {
            UserCharacterUI ui;
            if (i < uiPool.Count)
            {
                ui = uiPool[i];
            }
            else
            {
                ui = Instantiate(prefab, iconParent);
                uiPool.Add(ui);
            }

            ui.Set(characters[i]);
            ui.gameObject.SetActive(true);
        }
    }


}
