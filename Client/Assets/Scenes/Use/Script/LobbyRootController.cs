using Contracts.Protos;
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
    public GameObject panelLogin, panelMain, panelBattle, panelAdventure, panelShop, partySet, panelBattleMap, panelGachaShop;

    [Header("Popup Root (for Addressable popups)")]
    [SerializeField] private Transform popupRoot;
    private Dictionary<string, GameObject> _panels;
    private Dictionary<string, System.Action> _onShowActions;

    [HideInInspector] public int _currentBattleId;
    [HideInInspector] public StagePb _currentStage;

    public PingScheduler _scheduler;
    [SerializeField] private FadeInOut FadeInOut;
    public LoadingPopup Loading;


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
            ["Shop"] = panelShop,
            ["PartySet"] = partySet,
            ["BattleMap"] = panelBattleMap,
            ["GachaShop"] = panelGachaShop,
        };
        _onShowActions = new()
        {
            ["Login"] = OpenLoginPopup,
            ["Main"] = () => OpenLobbyPopup(),
            ["Battle"] = () => OpenBattleLobbyPopup(),
            ["Adventure"] = () => OpenAdventureLobbyPopup(),
            ["PartySet"] = () => OpenPartySetupPopup(),
            ["BattleMap"] = () => OpenBattleMapPopup(),
            ["GachaShop"] = () => OpenGachaShopPopup(),
        };

        btnLogin.onClick.AddListener(() => Show("Login"));
        btnMain.onClick.AddListener(() => Show("Main"));
        btnBattle.onClick.AddListener(() => Show("Battle"));
        btnAdventure.onClick.AddListener(() => Show("Adventure"));
        btnShop.onClick.AddListener(() => Show("Shop"));

    }

    public void Show(string key)
    {
        FadeInOut.Direct_FadeOut();
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

        _scheduler.gameObject.SetActive(false);

        FadeInOut.Start_FadeIn();
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
        if (popup == null) { Debug.LogError("LobbyPopup open failed"); return; }

        FadeInOut.Start_FadeIn();

        popup.Set_BattleLobbyBtn(() => Show("Battle"));
    }
    private async void OpenBattleLobbyPopup()
    {
        // Addressables 키 이름은 프로젝트에 맞게
        const string key = "BattleLobbyPopup";
        // 풀에서 열기
        var popupPool = UIPrefabPool.Instance as UIPopupPool;
        if (popupPool == null)
        {
            // 혹시 베이스만 살아있거나 초기화 순서 꼬였을 때 대비
            popupPool = FindObjectOfType<UIPopupPool>();
            if (popupPool == null) { Debug.LogError("UIPopupPool not found"); return; }
        }

        var popup = await popupPool.ShowPopupAsync<BattleLobbyPopup>(key, popupRoot);
        if (popup == null) { Debug.LogError("BattleLobbyPopup open failed"); return; }

        FadeInOut.Start_FadeIn();
        popup.Set_AdventureBtn(() => Show("Adventure"));
    }
    private async void OpenAdventureLobbyPopup()
    {
        // Addressables 키 이름은 프로젝트에 맞게
        const string key = "AdventureLobbyPopup";
        // 풀에서 열기
        var popupPool = UIPrefabPool.Instance as UIPopupPool;
        if (popupPool == null)
        {
            // 혹시 베이스만 살아있거나 초기화 순서 꼬였을 때 대비
            popupPool = FindObjectOfType<UIPopupPool>();
            if (popupPool == null) { Debug.LogError("UIPopupPool not found"); return; }
        }

        var popup = await popupPool.ShowPopupAsync<AdventureLobbyPopup>(key, popupRoot);
        if (popup == null) { Debug.LogError("BattleLobbyPopup open failed"); return; }

        FadeInOut.Start_FadeIn();
        popup.Set();
    }
    private async void OpenPartySetupPopup()
    {
        const string key = "PartySetupUI";
        var popupPool = UIPrefabPool.Instance as UIPopupPool;
        if (popupPool == null)
        {
            popupPool = FindObjectOfType<UIPopupPool>();
            if (popupPool == null) { Debug.LogError("UIPopupPool not found"); return; }
        }
        var popup = await popupPool.ShowPopupAsync<PartySetupPopup>(key, popupRoot);
        if (popup == null) { Debug.LogError("PartySetupPopup open failed"); return; }

        popup.Set(FadeInOut.Start_FadeIn);
    }
    private async void OpenBattleMapPopup()
    {
        const string key = "BattleMapPopup";
        var popupPool = UIPrefabPool.Instance as UIPopupPool;
        if (popupPool == null)
        {
            popupPool = FindObjectOfType<UIPopupPool>();
            if (popupPool == null) { Debug.LogError("UIPopupPool not found"); return; }
        }
        var popup = await popupPool.ShowPopupAsync<BattleMapPopup>(key, popupRoot);
        if (popup == null) { Debug.LogError("BattleMapPopup open failed"); return; }

        popup.Set(FadeInOut.Start_FadeIn);
    }

    private async void OpenGachaShopPopup()
    {
        const string key = "GachaPopupUI";
        var popupPool = UIPrefabPool.Instance as UIPopupPool;
        if (popupPool == null)
        {
            popupPool = FindObjectOfType<UIPopupPool>();
            if (popupPool == null) { Debug.LogError("UIPopupPool not found"); return; }
        }
        var popup = await popupPool.ShowPopupAsync<GachaShopPopup>(key, popupRoot);
        if (popup == null) { Debug.LogError("BattleMapPopup open failed"); return; }

        GachaBannerListPb listpb = new GachaBannerListPb();
        Debug.Log("listpb 추후 받아오기");

        popup.Set(FadeInOut.Start_FadeIn, listpb);
    }

}
