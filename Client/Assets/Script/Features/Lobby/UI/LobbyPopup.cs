using Client.Systems;
using Contracts.Protos;
using Game.Data;
using Game.Lobby;
using Game.Network;
using Game.UICommon;
using Lobby;
using Game.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPopup : UIPopup
{
    [Header("UI Refs")]
    [SerializeField] private UserProfileIUI ProfileUI;
    [SerializeField] private CurrencyUI CurrencyUI;

    [Header("Optional")]
    public LoadingSpinner Spinner;    // ������ ����
    public Popup Popup;               // ������ ����
    public ProtoHttpClient Http;      // ����� �ڵ� Ž��

    [Header("EachBtn")]
    [SerializeField] private Button CharactersBtn;
    [SerializeField] private Button InventoryBtn;
    [SerializeField] private Button AdventureBtn;
    [SerializeField] private Button GachaBtn;
    [SerializeField] private Button ShopBtn;

    private InventoryUI _inventoryPopup;
    private UserCharactersListUI _userCharacterPopup;

    [SerializeField] private Transform hiddenRoot;       // �̸� ������ �����

    private UIPopupPool _popupPool;

    private void Awake()
    {
        if (Http == null)
            Http = AppBootstrap.Instance != null
                ? AppBootstrap.Instance.Http
                : FindObjectOfType<AppBootstrap>()?.Http;
    }
    private async void OnEnable()
    {
        try
        {
            Initialize();
            _popupPool = UIPrefabPool.Instance as UIPopupPool
                         ?? FindObjectOfType<UIPopupPool>();

            if (_popupPool == null)
            {
                GameLogger.Error("[LobbyPopup] UIPopupPool not found");
                return;
            }

            var user = GameState.Instance.CurrentUser;
            if (user != null && user.UserProfilePb != null)
            {
                ApplyFromGameState();
            }
            else
            {
                GameLogger.Error($"[LobbyPopup] Lost user data: user={user == null}");
            }

            CharactersBtn.onClick.RemoveAllListeners();
            CharactersBtn.onClick.AddListener(ToggleUserCharacterPopup);

            InventoryBtn.onClick.RemoveAllListeners();
            InventoryBtn.onClick.AddListener(ToggleInventoryPopup);
            AddEvent_GachaShop();

            await PreloadPopups();
        }
        catch (Exception e)
        {
            GameLogger.Error($"[LobbyPopup] OnEnable failed: {e.Message}");
        }
    }
    private async Task PreloadPopups()
    {
        Transform popupRoot = this.transform;
        // �κ��丮
        if (_inventoryPopup == null)
        {
            const string invKey = "InventoryPopupUI";
            var go = await _popupPool.ShowPopupAsync<InventoryUI>(invKey, hiddenRoot);
            if (go != null)
            {
                _inventoryPopup = go;
                go.gameObject.SetActive(false);
                go.transform.parent = popupRoot;
            }
        }

        // ĳ���� ����Ʈ
        if (_userCharacterPopup == null)
        {
            const string charKey = "UserCharacterPopupUI";
            var go = await _popupPool.ShowPopupAsync<UserCharactersListUI>(charKey, hiddenRoot);
            if (go != null)
            {
                _userCharacterPopup = go;
                go.gameObject.SetActive(false);
                go.transform.parent = popupRoot;
            }
        }
    }
    private void ToggleInventoryPopup()
    {
        if (_inventoryPopup == null)
        {
            Debug.LogWarning("Inventory popup not loaded yet!");
            return;
        }

        bool active = _inventoryPopup.gameObject.activeSelf;
        _inventoryPopup.gameObject.SetActive(!active);
    }

    private void ToggleUserCharacterPopup()
    {
        if (_userCharacterPopup == null)
        {
            Debug.LogWarning("UserCharacter popup not loaded yet!");
            return;
        }

        bool active = _userCharacterPopup.gameObject.activeSelf;
        _userCharacterPopup.gameObject.SetActive(!active);
    }

    private void ApplyFromGameState()
    {
        var profile = GameState.Instance.CurrentUser.UserProfilePb;
        ProfileUI?.Set(profile);
        CurrencyUI?.Set(profile);
    }

    public override void Initialize()
    {
        base.Initialize();
        // ���ǳ� �⺻ ���α�
        if (Spinner) Spinner.gameObject.SetActive(false);
    }
    private IEnumerator CoRefreshLobby()
    {
        if (Http == null)
            yield break;

        SetLoading(true);

        // ��: ������ �ٽ� ��������
        UserProfilePb profile = null;
        yield return Http.Get(ApiRoutes.MeProfile, UserProfilePb.Parser, res =>
        {
            if (!res.Ok || res.Data == null)
            {
                Popup?.Show("������ ���� ����");
                return;
            }

            profile = res.Data;
            GameState.Instance.CurrentUser?.SetUserProfile(profile);
        });

        // ���������� UI�� �ݿ�
        if (profile != null)
            ApplyFromGameState();

        SetLoading(false);
    }

    private void SetLoading(bool on)
    {
        if (Spinner)
        {
            Spinner.gameObject.SetActive(on);
            Spinner.Show(on);
        }
    }
    public void Set_BattleLobbyBtn(Action onBattleLobbyClicked)
    {
        // Ȥ�� �ߺ� ������ ����
        AdventureBtn.onClick.RemoveAllListeners();

        // ���ڷ� ���� Action�� �����ʷ� ����
        AdventureBtn.onClick.AddListener(() =>
        {
            onBattleLobbyClicked?.Invoke();
        });
    }
    public void AddEvent_GachaShop()
    {
        GachaBtn.onClick.RemoveAllListeners();
        GachaBtn.onClick.AddListener(() => LobbyRootController.Instance.Show("GachaShop"));
    }
}
