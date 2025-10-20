using Contracts.Protos;
using Game.Auth;
using Game.Core;
using Game.Data;
using Game.Managers;
using Game.Network;
using Game.UICommon;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Game.Scenes.Login
{
    public class LoginSceneController : MonoBehaviour
    {
        [Header("UI")]
        public Button GuestLoginButton;
        public Button LoginButton;
        public TMP_InputField AccountInput;
        public TMP_InputField PasswordInput;
        public LoadingSpinner Spinner;
        public Popup Popup;

        [Header("Refs (선택: 비워두면 자동 탐색)")]
        public ProtoHttpClient Http;

        void Awake()
        {
            if (Http == null) Http = ProtoHttpClient.Instance ?? FindObjectOfType<ProtoHttpClient>();

            // 버튼 연결
            if (GuestLoginButton != null)
            {
                GuestLoginButton.onClick.RemoveListener(OnClickGuestLogin);
                GuestLoginButton.onClick.AddListener(OnClickGuestLogin);
            }
            if (LoginButton != null)
            {
                LoginButton.onClick.RemoveListener(OnClickLogin);
                LoginButton.onClick.AddListener(OnClickLogin);
            }
        }

        private void OnClickGuestLogin()
        {
            if (!isActiveAndEnabled) return;
            StartCoroutine(CoGuestLogin());
        }
        private void OnClickLogin()
        {
            if (!isActiveAndEnabled) return;

            var account = AccountInput != null ? AccountInput.text?.Trim() : "";
            var password = PasswordInput != null ? PasswordInput.text : "";

            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                Popup?.Show("아이디/비밀번호를 입력하세요.");
                return;
            }
            StartCoroutine(CoPasswordLogin(account, password));
        }

        private IEnumerator CoGuestLogin()
        {
            // 버튼 중복 입력 방지
            if (GuestLoginButton != null) GuestLoginButton.interactable = false;
            Spinner?.Show(true);

            // 1) 게스트 로그인 요청
            var req = new GuestAuthRequest
            {
                DeviceId = SystemInfo.deviceUniqueIdentifier
            };

            bool authOk = false;
            string playerId = null;
            string accessToken = null;
            string refreshToken = null;
            long serverUnix = 0;

            yield return Http.Post(ApiRoutes.AuthGuest, req, AuthResponse.Parser, (res) =>
            {
                if (!res.Ok)
                {
                    Spinner?.Show(false);
                    Popup?.Show($"로그인 실패: {res.Message}");
                    return;
                }

                authOk = true;
                playerId = res.Data.PlayerId;
                accessToken = res.Data.AccessToken;
                refreshToken = res.Data.RefreshToken;
                serverUnix = res.Data.ServerUnixMs;
            });

            if (!authOk)
            {
                if (GuestLoginButton != null) GuestLoginButton.interactable = true;
                yield break;
            }

            // 2) 토큰/서버시간 저장 + HTTP 헤더 주입
            GameState.Instance.SetServerTimeOffset(serverUnix);
            GameState.Instance.SaveAuth(playerId, accessToken, refreshToken);
            Http.SetToken(accessToken);

            // 3) 로비 진입 전 초기 데이터 (Bootstrap) 호출
            PlayerBootstrap bootstrap = null;
            yield return Http.Get(ApiRoutes.PlayerBootstrap, PlayerBootstrap.Parser, (res) =>
            {
                if (!res.Ok)
                {
                    Spinner?.Show(false);
                    Popup?.Show($"부트스트랩 실패: {res.Message}");
                    return;
                }
                bootstrap = res.Data;
            });

            if (bootstrap == null)
            {
                if (GuestLoginButton != null) GuestLoginButton.interactable = true;
                yield break;
            }

            // 4) 필요한 최소 상태 반영 
            GameState.Instance.SetNickname(bootstrap.Nickname);
            GameState.Instance.SetCurrencies(bootstrap.SoftCurrency, bootstrap.HardCurrency);

            Spinner?.Show(false);
            if (GuestLoginButton != null) GuestLoginButton.interactable = true;

            // 5) 로비로 전환
            SceneController.Instance.Go("Lobby");
        }
        private IEnumerator CoPasswordLogin(string account, string password)
        {
            if (GuestLoginButton) GuestLoginButton.interactable = false;
            if (LoginButton) LoginButton.interactable = false;
            Spinner?.Show(true);

            var req = new LoginAuthRequest { Account = account, Password = password };
            Debug.Log($"Account {account} Password {password}");
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
                playerId = res.Data.PlayerId; access = res.Data.AccessToken; refresh = res.Data.RefreshToken; serverMs = res.Data.ServerUnixMs;
            });

            // 보안: 메모리상 비밀번호 지우기 (UI도 클리어)
            if (PasswordInput) PasswordInput.text = "";

            yield return AfterAuth(ok, playerId, access, refresh, serverMs);
        }
        public IEnumerator AfterAuth(bool ok, string playerId, string access, string refresh, long serverMs)
        {
            if (!ok)
            {
                if (GuestLoginButton) GuestLoginButton.interactable = true;
                if (LoginButton) LoginButton.interactable = true;
                yield break;
            }

            GameState.Instance.SetServerTimeOffset(serverMs);
            GameState.Instance.SaveAuth(playerId, access, refresh);
            Http.SetToken(access);

            Debug.Log($"[PB Runtime] {typeof(Google.Protobuf.MessageParser).Assembly.FullName}");


            UserProfilePb profile = null;

            // /api/pb/me/profile 호출
            Debug.Log($"[CHECK] UserProfilePb.Parser is null? {(Contracts.Protos.UserProfilePb.Parser == null)}");
            yield return Http.Get(ApiRoutes.MeProfile, UserProfilePb.Parser, (ApiResult<UserProfilePb> res) =>
            {
                if (!res.Ok)
                {
                    Debug.LogError($"[프로필 실패] code={res.StatusCode}, err={res.ErrorCode}\n{res.Message}");
                    Popup?.Show($"프로필 불러오기 실패: {res.Message}");
                    return;  
                }

                if (res.Data == null) { Debug.LogError("Data==null"); return; }

                profile = res.Data;
                GameState.Instance.InitUser(int.Parse(GameState.Instance.PlayerId), profile.Nickname, profile.Level);
                GameState.Instance.CurrentUser ??= new UserData(int.Parse(playerId), "FailedLogin", 1);

                Debug.Log("프로필 셋팅");
                GameState.Instance.CurrentUser.SetUserProfile(profile);
            });

            PlayerBootstrap boot = null;
            yield return Http.Get(ApiRoutes.PlayerBootstrap, PlayerBootstrap.Parser, (res) =>
            {
                if (!res.Ok) 
                { Spinner?.Show(false); Popup?.Show($"부트스트랩 실패: {res.Message}");

                    Debug.Log("[ 부트스트랩 실패 ] @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                    return; }
                boot = res.Data;
            });

            if (boot == null)
            {
                if (GuestLoginButton) GuestLoginButton.interactable = true;
                if (LoginButton) LoginButton.interactable = true;
                yield break;
            }

            GameState.Instance.SetNickname(boot.Nickname);
            GameState.Instance.SetCurrencies(boot.SoftCurrency, boot.HardCurrency);

            // 3) 인벤토리 불러오기
            ListUserInventoryResponse invRes = null;

            yield return Http.Get(ApiRoutes.UserInventoryList(int.Parse(GameState.Instance.PlayerId)), ListUserInventoryResponse.Parser,
    (res) =>
    {
        if (!res.Ok)
        {
            Spinner?.Show(false);
            Popup?.Show($"인벤토리 불러오기 실패: {res.Message}");
            Debug.Log($"status={res.StatusCode}, ");
            Debug.Log($"err={res.Message}");
            return;
        }

        invRes = res.Data;
    }); 
            if (invRes != null)
            {
                GameState.Instance.CurrentUser.SyncInventory(invRes.Items);
            }

            // 4) 캐릭터 불러오기 
            UserCharacterListPb chaRes = null;

            yield return Http.Get(ApiRoutes.UserCharacterList(int.Parse(GameState.Instance.PlayerId)), UserCharacterListPb.Parser,
                (res) =>
                {
                    if (!res.Ok)
                    {
                        chaRes = res.Data ?? new UserCharacterListPb();
                        return;
                    }
                    var isEmptySuccess =
          res.StatusCode >= 200 && res.StatusCode < 300 &&
          (res.Data == null || string.Equals(res.Message, "Response body is empty", StringComparison.OrdinalIgnoreCase));

                    if (isEmptySuccess)
                    {
                        chaRes = new UserCharacterListPb(); // 빈 characters
                        return;
                    }

                    Spinner?.Show(false);
                    Popup?.Show($"유저 캐릭터 불러오기 실패: {res.Message}");
                    Debug.Log($"[ERROR] (UserCharacters) status={res.StatusCode}");
                    Debug.Log($"[ERROR] (UserCharacters) err={res.Message}");

                    chaRes = res.Data;
                });

            if(chaRes != null)
            {
                GameState.Instance.CurrentUser.SyncCharacters(chaRes.Characters);
            }

            Spinner?.Show(false);
            if (GuestLoginButton) GuestLoginButton.interactable = true;
            if (LoginButton) LoginButton.interactable = true;

            SceneController.Instance.Go("Lobby");
        }
    }
}