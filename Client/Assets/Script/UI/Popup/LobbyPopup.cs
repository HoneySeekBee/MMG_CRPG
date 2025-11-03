using Client.Systems;
using Contracts.Protos;
using Game.Data;
using Game.Lobby;
using Game.Network;
using Game.UICommon;
using Lobby;
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
    public LoadingSpinner Spinner;    // 없으면 무시
    public Popup Popup;               // 없으면 무시
    public ProtoHttpClient Http;      // 비워도 자동 탐색

    [Header("EachBtn")]
    [SerializeField] private Button CharactersBtn;
    [SerializeField] private Button InventoryBtn;
    [SerializeField] private Button AdventureBtn;
    [SerializeField] private Button GachaBtn;
    [SerializeField] private Button ShopBtn;

    private InventoryUI _inventoryPopup;
    private UserCharactersListUI _userCharacterPopup;

    [SerializeField] private Transform hiddenRoot;       // 미리 만들어둘 숨김용

    private UIPopupPool _popupPool;
    private bool _initialized;

    private void Awake()
    {
        if (Http == null)
            Http = AppBootstrap.Instance != null
                ? AppBootstrap.Instance.Http
                : FindObjectOfType<AppBootstrap>()?.Http;
    }
    private async void OnEnable()
    {
        Initialize();
        _popupPool = UIPrefabPool.Instance as UIPopupPool
                     ?? FindObjectOfType<UIPopupPool>();

        if (_popupPool == null)
        {
            Debug.LogError("UIPopupPool not found");
            return;
        }

        var user = GameState.Instance.CurrentUser;
        if (user != null && user.UserProfilePb != null)
        {
            ApplyFromGameState();
        }

        //  버튼 이벤트 연결
        CharactersBtn.onClick.RemoveAllListeners();
        CharactersBtn.onClick.AddListener(ToggleUserCharacterPopup);

        InventoryBtn.onClick.RemoveAllListeners();
        InventoryBtn.onClick.AddListener(ToggleInventoryPopup);

        //  초기 로드 (비활성화 상태로)
        await PreloadPopups();
    }
    private async Task PreloadPopups()
    {
        Transform popupRoot = this.transform;
        // 인벤토리
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

        // 캐릭터 리스트
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
        if (_initialized) return;
        _initialized = true;

        // 스피너 기본 꺼두기
        if (Spinner) Spinner.gameObject.SetActive(false);
    }
    private IEnumerator CoRefreshLobby()
    {
        if (Http == null)
            yield break;

        SetLoading(true);

        // 예: 프로필 다시 가져오기
        UserProfilePb profile = null;
        yield return Http.Get(ApiRoutes.MeProfile, UserProfilePb.Parser, res =>
        {
            if (!res.Ok || res.Data == null)
            {
                Popup?.Show("프로필 갱신 실패");
                return;
            }

            profile = res.Data;
            GameState.Instance.CurrentUser?.SetUserProfile(profile);
        });

        // 성공했으면 UI에 반영
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
}
