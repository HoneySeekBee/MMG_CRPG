using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRootController : MonoBehaviour
{
    public static LobbyRootController Instance { get; private set; }
    public Button btnLogin, btnMain, btnBattle, btnAdventure, btnShop;
    public GameObject panelLogin, panelMain, panelBattle, panelAdventure, panelShop;


    private Dictionary<string, GameObject> _panels;

    private void Awake()
    {
        Instance = this;
        _panels = new()
        {
            ["Login"] = panelLogin,
            ["Main"] = panelMain,
            ["Battle"] = panelBattle,
            ["Adventure"] = panelAdventure,
            ["Shop"] = panelShop
        };

        btnLogin.onClick.AddListener(() => Show("Login"));
        btnMain.onClick.AddListener(() => Show("Main"));
        btnBattle.onClick.AddListener(() => Show("Battle"));
        btnAdventure.onClick.AddListener(() => Show("Adventure"));
        btnShop.onClick.AddListener(() => Show("Shop"));
    }

    private void Start()
    {
        Show("Main");   // ±âº» ÅÇ
        Debug.Log("[LobbyRoot] Ready");
    }

    public void Show(string key)
    {
        foreach (var kv in _panels) kv.Value.SetActive(kv.Key == key);
    }

}
