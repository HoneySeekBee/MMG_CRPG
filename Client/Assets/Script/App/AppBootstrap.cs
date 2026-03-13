using Cache;
using Contracts.Protos;
using Game.Auth;
using Game.Data;
using Game.Logging;
using Game.Managers;
using Game.Network;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client.Systems
{
    public class AppBootstrap : MonoBehaviour
    {
        public static AppBootstrap Instance { get; private set; }
        [Header("References")]
        public ApiConfig ApiConfig;
        public Game.UICommon.LoadingSpinner Spinner; // Loading spinner reference
        public Game.UICommon.Popup Popup;            // Popup reference

        [SerializeField] private AudioListener audioListner;
        public ProtoHttpClient Http { get; private set; }

        // Separate HTTP client pointing to CombatServer (Tick/Command/Speed)
        public ProtoHttpClient CombatHttp { get; private set; }

        public ProtoAuthService AuthService { get; private set; }
        private bool _authRedirecting;
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("=== [AppBootstrap] Awake ===");

            // Initialize scene manager
            if (SceneController.Instance == null)
                new GameObject("SceneController").AddComponent<SceneController>();
            if (GameState.Instance == null)
                new GameObject("GameState").AddComponent<GameState>();

            // Network setup
            Http = new ProtoHttpClient(ApiConfig);

            // CombatServer client - uses same config but different BaseUrl
            var combatConfig = ScriptableObject.CreateInstance<ApiConfig>();
            combatConfig.BaseUrl = !string.IsNullOrEmpty(ApiConfig.CombatServerUrl)
                ? ApiConfig.CombatServerUrl
                : ApiConfig.BaseUrl; // Fallback to same server if not set
            combatConfig.DefaultTimeoutSec = ApiConfig.DefaultTimeoutSec;
            combatConfig.RetryCount = ApiConfig.RetryCount;
            combatConfig.RetryBackoffSec = ApiConfig.RetryBackoffSec;
            CombatHttp = new ProtoHttpClient(combatConfig);
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

            Debug.Log("[AppBootstrap] (TODO) Addressables initialization skipped");
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var count = FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            SetAudioListener(count <= 1);
        }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        private void SetAudioListener(bool _enable)
        {
            if (audioListner != null)
                audioListner.enabled = _enable;
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

            yield return CheckServerStatus();
            _ = AudioManager.Instance.PlayBGM("bgm_lobby_academy");
            yield return LoadEssentialCaches();       // Phase 1: Essential lobby caches
            StartCoroutine(LoadBattleCaches());       // Phase 2: Battle caches (background)

            yield return TryAutoLogin();

            Spinner?.Show(false);
            Debug.Log("=== [AppBootstrap] Boot Complete ===");
        }

        IEnumerator CheckServerStatus()
        {
            Debug.Log("[AppBootstrap] Checking server status...");
            bool done = false;

            yield return Http.Get(ApiRoutes.Status, StatusPb.Parser, res =>
            {
                done = true;
                if (!res.Ok)
                {
                    Debug.LogError($"[Status] Failed: {res.Message}");
                    Popup?.Show($"Network error: {res.Message}");
                    return;
                }

                var s = res.Data;

                if (GameState.Instance == null)
                    new GameObject("GameState").AddComponent<GameState>();

                GameState.Instance.SetServerTimeOffset(s.ServerUnixMs);
                if (s.Maintenance) { Popup?.Show(string.IsNullOrEmpty(s.Message) ? "Server is under maintenance." : s.Message); }
                if (s.ForceUpdate) { Popup?.Show("An update is required. Please visit the store."); }
                Debug.Log("[AppBootstrap] Server status OK");
            });

            while (!done) yield return null;
        }

        IEnumerator LoadEssentialCaches()
        {
            var routines = new (IEnumerator routine, string label)[]
            {
                (MasterDataCache.Instance.CoLoadMasterData(Http, Popup), "MasterData"),
                (ItemCache.Instance.CoLoadItemData(Http, Popup),         "Item"),
                (CharacterCache.Instance.CoLoadCharacterCache(Http, Popup), "Character"),
                (UIImageCache.Instance.PreloadAllUISprites(),             "UIImage"),
            };
            yield return RunParallel(routines);
        }
        IEnumerator LoadBattleCaches()
        {
            var routines = new (IEnumerator routine, string label)[]
            {
                (SkillCache.Instance.CoLoadSkillData(Http, Popup),            "Skill"),
                (BattleContentsCache.Instance.CoLoadContents(Http, Popup),    "BattleContents"),
                (MonsterCache.Instance.CoLoadMonsterCache(Http, Popup),       "Monster"),
            };
            yield return RunParallel(routines);
        }
        IEnumerator RunParallel((IEnumerator routine, string label)[] routines)
        {
            int remaining = routines.Length;
            foreach (var (routine, label) in routines)
                StartCoroutine(WrapTimed(routine, label, () => remaining--));
            yield return new WaitUntil(() => remaining <= 0);
        }

        IEnumerator Wrap(IEnumerator routine, Action onDone)
        {
            yield return routine;
            onDone?.Invoke();
        }
        IEnumerator WrapTimed(IEnumerator routine, string label, Action onDone)
        {
            float start = Time.realtimeSinceStartup;
            yield return routine;
            Debug.Log($"[Boot] {label}: {Time.realtimeSinceStartup - start:F2}s");
            onDone?.Invoke();
        }
        IEnumerator TryAutoLogin()
        {
            GameLogger.Info("[AppBootstrap] Try Auto Login");

            GameState.Instance.LoadFromPrefs();
            var refresh = GameState.Instance.RefreshToken;

            if (string.IsNullOrEmpty(refresh))
            {
                GameState.Instance.SetNeedLogin();
                Http.ClearToken();

                yield return SceneController.Instance.GoAsync("LobbyRoot");
                yield return null;

                LobbyRootController.Instance.Show("Login");
                yield break;
            }

            // 1) Refresh
            bool refreshOk = false;
            string playerId = null, access = null, newRefresh = null;
            long serverMs = 0;

            yield return AuthService.Refresh(refresh, res =>
            {
                if (!res.Ok)
                {
                    GameLogger.Warn($"[AppBootstrap] [Auth Refresh] Failed: {res.Message}");
                    return;
                }

                refreshOk = true;
                playerId = res.Data.PlayerId;
                access = res.Data.AccessToken;
                newRefresh = res.Data.RefreshToken;
                serverMs = res.Data.ServerUnixMs;
                GameLogger.Info($"[AppBootstrap] [Auth Refresh] : {res.Data.PlayerId} {res.Data.AccessToken}");
            });

            if (!refreshOk)
            {
                GameState.Instance.SetNeedLogin();
                Http.ClearToken();

                yield return SceneController.Instance.GoAsync("LobbyRoot");
                yield return null;

                LobbyRootController.Instance.Show("Login");
                yield break;
            }

            var bootstrap = new AuthBootstrapper(Http, Popup);
            GameLogger.Info($"player ID : {playerId}");
            yield return bootstrap.CoBootstrapAfterToken(playerId, access, newRefresh, serverMs);

            bool bootOk = !string.IsNullOrEmpty(GameState.Instance.AccessToken)
                          && GameState.Instance.CurrentUser != null;

            if (bootOk == false)
            {
                GameLogger.Warn($"[AppBootstrap] Bootstrap-> Login  {GameState.Instance.CurrentUser == null}");

                GameState.Instance.SetNeedLogin();
                Http.ClearToken();

                yield return SceneController.Instance.GoAsync("LobbyRoot");
                yield return null;
                LobbyRootController.Instance.Show("Login");
                yield break;
            }

            GameLogger.Info("[AppBootstrap] -> Main");
            ResetAuthRedirect();

            yield return SceneController.Instance.GoAsync("LobbyRoot");
            yield return null;
            LobbyRootController.Instance.Show("Main");
        }
        public void ResetAuthRedirect()
        {
            _authRedirecting = false;
        }
    }
}
