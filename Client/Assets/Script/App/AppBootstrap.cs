using Cache;
using Contracts.Protos;
using Game.Auth;
using Game.Data;
using Game.Managers;
using Game.Network;
using System;
using System.Collections; 
using UnityEngine;

namespace Client.Systems
{
    public class AppBootstrap : MonoBehaviour
    {
        public static AppBootstrap Instance { get; private set; }
        [Header("References")]
        public ApiConfig ApiConfig;
        public Game.UICommon.LoadingSpinner Spinner; // 있으면 연결
        public Game.UICommon.Popup Popup;            // 있으면 연결

        public ProtoHttpClient Http { get; private set; }
        public ProtoAuthService AuthService { get; private set; }
        private bool _authRedirecting;
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("=== [AppBootstrap] Awake ===");

            // 전역 매니저 보장
            if (SceneController.Instance == null)
                new GameObject("SceneController").AddComponent<SceneController>();
            if (GameState.Instance == null)
                new GameObject("GameState").AddComponent<GameState>();

            // 네트워크 준비
            Http = new ProtoHttpClient(ApiConfig);
            Http.OnUnauthorized += code =>
            {
                if (_authRedirecting) return;
                _authRedirecting = true;

                Debug.LogWarning($"[Auth] Unauthorized({code}) -> NeedLogin");
                GameState.Instance.SetNeedLogin();
                Http.ClearToken();

                Instance.StartCoroutine(GoLoginFlow());
            };
            AuthService = new ProtoAuthService(Http);

            Debug.Log("[AppBootstrap] (TODO) Addressables 초기화 예정");
        }
        private IEnumerator GoLoginFlow()
        {
            yield return SceneController.Instance.GoAsync("LobbyRoot");
            yield return null;
            if (LobbyRootController.Instance != null)
                LobbyRootController.Instance.Show("Login");
        }
        IEnumerator Start()
        {  
            Debug.Log("=== [AppBootstrap] Start: Boot Begin ===");
            Spinner?.Show(true);

            yield return CheckServerStatus();   // 점검/업데이트 확인
            AudioManager.Instance.PlayBGM("bgm_lobby_academy");
            yield return LoadCaches();          // 마스터/스프라이트 등 (더미)

            yield return TryAutoLogin();        // Refresh 성공→Lobby, 실패→Login

            Spinner?.Show(false);
            Debug.Log("=== [AppBootstrap] Boot Complete ===");
        }

        IEnumerator CheckServerStatus()
        {
            Debug.Log("[AppBootstrap] 서버 상태 확인...");
            bool done = false;

            yield return Http.Get(ApiRoutes.Status, StatusPb.Parser, res =>
            {
                done = true;
                if (!res.Ok)
                {
                    Debug.LogError($"[Status] 실패: {res.Message}");
                    Popup?.Show($"네트워크 오류: {res.Message}");
                    return;
                }

                var s = res.Data;

                if (GameState.Instance == null)
                    new GameObject("GameState").AddComponent<GameState>();

                GameState.Instance.SetServerTimeOffset(s.ServerUnixMs);
                if (s.Maintenance) { Popup?.Show(string.IsNullOrEmpty(s.Message) ? "점검 중입니다." : s.Message); }
                if (s.ForceUpdate) { Popup?.Show("새 버전이 필요합니다. 스토어로 이동해주세요."); }
                Debug.Log("[AppBootstrap] 서버 정상");
            });

            while (!done) yield return null;
        }

        IEnumerator LoadCaches()
        {
            Debug.Log("[AppBootstrap] 캐시 로드 시작");
            bool done1 = false,
                done2 = false,
                done3 = false,
                done4 = false,
                done5 = false,
                done6 = false, 
                done8 = false;

            StartCoroutine(Wrap(MasterDataCache.Instance.CoLoadMasterData(Http, Popup), () => done1 = true));
            StartCoroutine(Wrap(ItemCache.Instance.CoLoadItemData(Http, Popup), () => done2 = true));
            StartCoroutine(Wrap(CharacterCache.Instance.CoLoadCharacterCache(Http, Popup), () => done3 = true));
            StartCoroutine(Wrap(SkillCache.Instance.CoLoadSkillData(Http, Popup), () => done4 = true));
            StartCoroutine(Wrap(UIImageCache.Instance.PreloadAllUISprites(), () => done5 = true));
            StartCoroutine(Wrap(BattleContentsCache.Instance.CoLoadContents(Http, Popup), () => done6 = true));
            StartCoroutine(Wrap(MonsterCache.Instance.CoLoadMonsterCache(Http, Popup), () => done8 = true));
            yield return new WaitUntil(() => done1 && done2 && done3 && done4 && done5 && done6 && done8); 
            Debug.Log("[AppBootstrap] 캐시 로드 완료");
        }
        // 각 코루틴 끝났을 때 콜백 호출하도록 래핑
        IEnumerator Wrap(IEnumerator routine, Action onDone)
        {
            yield return routine;
            onDone?.Invoke();
        }
        IEnumerator TryAutoLogin()
        {
            Debug.Log("[AppBootstrap] 자동 로그인 시도");

            GameState.Instance.LoadFromPrefs();

            var refresh = GameState.Instance.RefreshToken;

            if(string.IsNullOrEmpty(refresh)) // 리프레시 토큰 없음.
            {
                Debug.Log("[AppBootstrap] : 리프레시 토큰 없음");
                GameState.Instance.SetNeedLogin();
                yield return SceneController.Instance.GoAsync("LobbyRoot"); 
                LobbyRootController.Instance.Show("Login");
                yield break;
            }

            bool ok = false;
             
            // Refresh 요청
            yield return AuthService.Refresh(refresh, res =>
            {
                if (!res.Ok)
                {
                    Debug.LogWarning($"[AppBootstrap] [Auth Refresh] 실패: {res.Message}");
                    GameState.Instance.SetNeedLogin();
                    Http.ClearToken();// 토큰 제거하기 
                    return;
                }

                ok = true;
                ResetAuthRedirect();
                Http.SetToken(res.Data.AccessToken);
                GameState.Instance.SaveAuth(res.Data.PlayerId, res.Data.AccessToken, res.Data.RefreshToken);
                Debug.Log("[AppBootstrap] 자동 로그인 성공");
            });

            yield return SceneController.Instance.GoAsync("LobbyRoot");

            LobbyRootController.Instance.Show(ok ? "Main" : "Login");
        }
        public void ResetAuthRedirect()
        {
            _authRedirecting = false;
        }
    } 
}