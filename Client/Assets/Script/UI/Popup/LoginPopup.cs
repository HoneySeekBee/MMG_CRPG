using Client.Systems;
using Contracts.Protos;
using Game.Core;
using Game.Data;
using Game.Managers;
using Game.Network;
using Game.UICommon;
using Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 

public class LoginPopup : UIPopup
{
    public struct LoginResult
    {
        public bool Ok;
        public UserProfilePb Profile;
        public PlayerBootstrap Boot;
    }

    [Header("UI")]
    [SerializeField] private Button LoginButton;
    [SerializeField] private TMP_InputField AccountInput;
    [SerializeField] private TMP_InputField PasswordInput;
    [SerializeField] private LoadingSpinner Spinner;
    [SerializeField] private Popup Popup;

    [Header("Refs (선택: 비워두면 자동 탐색)")]
    public ProtoHttpClient Http;

    private bool _busy; 
    private Coroutine _running;
    public event Action<LoginResult> OnLoginCompleted;

    // [1] 로딩 상태를 한 곳에서 관리하기
    private void SetLoading(bool on)
    {
        Spinner?.Show(on);
        if (LoginButton)
            LoginButton.interactable = !on;
    }

    // [2] 안전한 PlayerId 파싱하기
    private bool TryGetPlayerId(out int pid)
    {
        pid = 0;

        if (int.TryParse(GameState.Instance.PlayerId, out pid))
            return true;

        return false;
    }

    private void OnEnable()
    {
        Initialize();

        if (LoginButton)
            LoginButton.onClick.AddListener(OnClickLogin);

        Http ??= AppBootstrap.Instance.Http;
    }

    private void OnDisable()
    {
        if (LoginButton)
            LoginButton.onClick.RemoveListener(OnClickLogin);

        if (_running != null)
        {
            StopCoroutine(_running);
            _running = null;
            _busy = false;
            SetLoading(false);
        }
    }
    private void OnClickLogin()
    {
        if (!isActiveAndEnabled || _busy) return;

        var account = AccountInput ? (AccountInput.text?.Trim() ?? "") : "";
        var password = PasswordInput ? (PasswordInput.text ?? "") : "";

        if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
        {
            Popup?.Show("아이디/비밀번호를 입력하세요.");
            return;
        }
        _running = StartCoroutine(CoPasswordLogin(account, password)); 
    }
    private IEnumerator CoPasswordLogin(string account, string password)
    {
        _busy = true;
        SetLoading(true);

        var req = new LoginAuthRequest { Account = account, Password = password };

        bool ok = false;
        string playerId = null, access = null, refresh = null;
        long serverMs = 0;
         
        yield return Http.Post(ApiRoutes.AuthLogin, req, AuthResponse.Parser, (res) =>
        {
            if (!res.Ok)
            {
                Spinner?.Show(false);
                Popup?.Show(res.StatusCode == 401 ? "계정 또는 비밀번호가 올바르지 않습니다." : $"로그인 실패: {res.Message}");
                return;
            }
            Debug.Log($"[AUTH] access len={res.Data.AccessToken?.Length}, player={res.Data.PlayerId}");

            ok = true;
            playerId = res.Data.PlayerId; 
            access = res.Data.AccessToken; 
            refresh = res.Data.RefreshToken; 
            serverMs = res.Data.ServerUnixMs;
        });

        // 보안: 메모리상 비밀번호 지우기 (UI도 클리어)
        if (PasswordInput) PasswordInput.text = "";

        yield return AfterAuth(ok, playerId, access, refresh, serverMs);
        SetLoading(false);
        _busy = false;
        _running = null;
    }
    public IEnumerator AfterAuth(bool ok, string playerId, string access, string refresh, long serverMs)
    {
        if (!ok)
        {
            SetLoading(false);
            _busy = false;
            yield break;
        }

        GameState.Instance.SetServerTimeOffset(serverMs);
        GameState.Instance.SaveAuth(playerId, access, refresh);
        Http.SetToken(access);
         
        UserProfilePb profile = null;
        // /api/pb/me/profile 호출 
        yield return Http.Get(ApiRoutes.MeProfile, UserProfilePb.Parser, (ApiResult<UserProfilePb> res) =>
        {
            if (!res.Ok || res.Data == null)
            {
                Popup?.Show($"프로필 불러오기 실패: {res.Message}");
                return;
            }
            profile = res.Data;
            int pid;
            if (!TryGetPlayerId(out pid))
                int.TryParse(playerId, out pid);

            GameState.Instance.InitUser(pid, profile.Nickname, profile.Level);
            GameState.Instance.CurrentUser ??= new UserData(pid, "Unknown", 1);
            GameState.Instance.CurrentUser.SetUserProfile(profile);
        }); 
        yield return Http.Get(ApiRoutes.UserStageProgress, MyStageProgressListPb.Parser, (ApiResult<MyStageProgressListPb> res) =>
        {
            if (!res.Ok)
            {
                Debug.LogError($"[UserStageProgress] 요청 실패: {res.Message}");
                Popup?.Show($"스테이지 불러오기 실패: {res.Message}");
                return;
            }

            if (res.Data == null)
            {
                Debug.LogWarning("[UserStageProgress] Data가 null입니다 (서버 응답 없음)");
                return;
            }

            // 진행 데이터가 없는 경우 = 신규 유저
            if (res.Data.Progresses.Count == 0)
            {
                Debug.Log("[UserStageProgress] 진행 데이터 없음 (신규 유저)");
                var user = GameState.Instance.CurrentUser;
                if (user != null)
                    user.StageProgress.Sync(res.Data);
                return;
            }

            // 정상 데이터 있을 때만 싱크
            Debug.Log($"[UserStageProgress] {res.Data.Progresses.Count}개 스테이지 진행 데이터 로드됨");
            GameState.Instance.CurrentUser?.SyncStageProgress(res.Data);
            // GameState에 싱크
            GameState.Instance.CurrentUser?.SyncStageProgress(res.Data);
        });
        if (profile == null)
        {
            SetLoading(false);
            _busy = false;
            yield break; // 실패 처리
        }
        PlayerBootstrap boot = null;
        yield return Http.Get(ApiRoutes.PlayerBootstrap, PlayerBootstrap.Parser, (res) =>
        {
            if (!res.Ok || res.Data == null)
            {
                Popup?.Show($"부트스트랩 실패: {res.Message}");
                return;
            }
            boot = res.Data;
        });

        if (boot == null)
        {
            SetLoading(false);
            _busy = false;
            yield break;
        }

        GameState.Instance.SetNickname(boot.Nickname);
        GameState.Instance.SetCurrencies(boot.SoftCurrency, boot.HardCurrency);


        // 3) 인벤토리 불러오기
        ListUserInventoryResponse invRes = null;
        yield return Http.Get(ApiRoutes.UserInventoryList(int.Parse(GameState.Instance.PlayerId)), ListUserInventoryResponse.Parser, (res) =>
        {
            if (res.Ok && res.Data != null)
                invRes = res.Data;
            else if (!res.Ok)
                Popup?.Show($"인벤토리 불러오기 실패: {res.Message}");
        });

        Debug.Log("인벤토리 불러옴" + invRes.Items.Count);
        foreach (var item in invRes.Items)
        {
            Debug.Log($"유저 보유 아이템 {item.Id} {item.ItemId}");
        }
        if (invRes != null)
            GameState.Instance.CurrentUser.SyncInventory(invRes.Items);

        // 4) 캐릭터 불러오기 
        UserCharacterListPb chaRes = null;
        yield return Http.Get(ApiRoutes.UserCharacterList(int.Parse(GameState.Instance.PlayerId)), UserCharacterListPb.Parser, (res) =>
        {
            if (res.Ok)
            {
                // 204/빈 바디 허용
                chaRes = res.Data ?? new UserCharacterListPb();
                return;
            }
            // 실패만 메시지
            Popup?.Show($"유저 캐릭터 불러오기 실패: {res.Message}");
        });


        OnLoginCompleted?.Invoke(new LoginResult
        {
            Ok = true,
            Profile = profile,
            Boot = boot
        });
        LobbyRootController.Instance._scheduler.gameObject.SetActive(true);

        if (chaRes != null)
            GameState.Instance.CurrentUser.SyncCharacters(chaRes.Characters);
          
    }
}
