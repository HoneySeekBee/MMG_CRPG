using Lobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRootController : MonoBehaviour
{
    public static LobbyRootController Instance { get; private set; }
    [Header("Tab Buttons")]
    public Button btnLogin, btnMain, btnBattle, btnAdventure, btnShop;
    [Header("Panels (Static)")]
    public GameObject panelLogin, panelMain, panelBattle, panelAdventure, panelShop;

    [Header("Popup Root (for Addressable popups)")]
    [SerializeField] private Transform popupRoot;

    private Dictionary<string, GameObject> _panels;
    private Dictionary<string, System.Action> _onShowActions; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        { 
            Destroy(gameObject);
            return; 
        }
        Instance = this;

        _panels = new()
        {
            ["Login"] = panelLogin,
            ["Main"] = panelMain,
            ["Battle"] = panelBattle,
            ["Adventure"] = panelAdventure,
            ["Shop"] = panelShop
        }; 
        _onShowActions = new()
        {
            ["Login"] = OpenLoginPopup,
            ["Main"] = () => OpenLobbyPopup(),
            // ["Shop"] = () => ShowShopTips(),
        };

        btnLogin.onClick.AddListener(() => Show("Login"));
        btnMain.onClick.AddListener(() => Show("Main"));
        btnBattle.onClick.AddListener(() => Show("Battle"));
        btnAdventure.onClick.AddListener(() => Show("Adventure"));
        btnShop.onClick.AddListener(() => Show("Shop"));
         
    }

    private void Start()
    { 
    }

    public void Show(string key)
    {
        popupRoot = _panels[key].transform;
        foreach (var kv in _panels)
        {
            kv.Value.SetActive(kv.Key == key);
        }
        if (_onShowActions.TryGetValue(key, out var action))
        {
            action?.Invoke();
        }
    }
    private async void OpenLoginPopup()
    {
        // Addressables 키 이름은 프로젝트에 맞게
        const string key = "LoginPopup";

        // 풀에서 열기
        var popupPool = UIPrefabPool.Instance as UIPopupPool;
        if (popupPool == null)
        {
            // 혹시 베이스만 살아있거나 초기화 순서 꼬였을 때 대비
            popupPool = FindObjectOfType<UIPopupPool>();
            if (popupPool == null) { Debug.LogError("UIPopupPool not found"); return; }
        }

        var popup = await popupPool.ShowPopupAsync<LoginPopup>(key, popupRoot);
        if (popup == null) { Debug.LogError("LoginPopup open failed"); return; }

        // 완료 이벤트 구독
        popup.OnLoginCompleted += async result =>
        {
            if (!result.Ok) return;

            // 원하는 패널로 전환
            Show("Main");

            // 팝업 닫기(풀에 반납)
            await popupPool.HidePopupAsync(key, popup);
        };
    }
    private async void OpenLobbyPopup()
    {
        // Addressables 키 이름은 프로젝트에 맞게
        const string key = "LobbyPopupUI";
        // 풀에서 열기
        var popupPool = UIPrefabPool.Instance as UIPopupPool;
        if (popupPool == null)
        {
            // 혹시 베이스만 살아있거나 초기화 순서 꼬였을 때 대비
            popupPool = FindObjectOfType<UIPopupPool>();
            if (popupPool == null) { Debug.LogError("UIPopupPool not found"); return; }
        }

        var popup = await popupPool.ShowPopupAsync<LobbyPopup>(key, popupRoot);
        if (popup == null) { Debug.LogError("LoginPopup open failed"); return; }
         
    }
}
