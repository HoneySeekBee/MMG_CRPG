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
        public Game.UICommon.LoadingSpinner Spinner; // ������ ����
        public Game.UICommon.Popup Popup;            // ������ ����

        [SerializeField] private AudioListener audioListner;
        public ProtoHttpClient Http { get; private set; }
        public ProtoAuthService AuthService { get; private set; }
        private bool _authRedirecting;
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("=== [AppBootstrap] Awake ===");

            // ���� �Ŵ��� ����
            if (SceneController.Instance == null)
                new GameObject("SceneController").AddComponent<SceneController>();
            if (GameState.Instance == null)
                new GameObject("GameState").AddComponent<GameState>();

            // ��Ʈ��ũ �غ�
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

            Debug.Log("[AppBootstrap] (TODO) Addressables �ʱ�ȭ ����");
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

            yield return CheckServerStatus();   // ����/������Ʈ Ȯ��
            _ = AudioManager.Instance.PlayBGM("bgm_lobby_academy");
            yield return LoadCaches();          // ������/��������Ʈ �� (����)

            yield return TryAutoLogin();        // Refresh ������Lobby, ���С�Login

            Spinner?.Show(false);
            Debug.Log("=== [AppBootstrap] Boot Complete ===");
        }

        IEnumerator CheckServerStatus()
        {
            Debug.Log("[AppBootstrap] ���� ���� Ȯ��...");
            bool done = false;

            yield return Http.Get(ApiRoutes.Status, StatusPb.Parser, res =>
            {
                done = true;
                if (!res.Ok)
                {
                    Debug.LogError($"[Status] ����: {res.Message}");
                    Popup?.Show($"��Ʈ��ũ ����: {res.Message}");
                    return;
                }

                var s = res.Data;

                if (GameState.Instance == null)
                    new GameObject("GameState").AddComponent<GameState>();

                GameState.Instance.SetServerTimeOffset(s.ServerUnixMs);
                if (s.Maintenance) { Popup?.Show(string.IsNullOrEmpty(s.Message) ? "���� ���Դϴ�." : s.Message); }
                if (s.ForceUpdate) { Popup?.Show("�� ������ �ʿ��մϴ�. ������ �̵����ּ���."); }
                Debug.Log("[AppBootstrap] ���� ����");
            });

            while (!done) yield return null;
        }

        IEnumerator LoadCaches()
        {
            Debug.Log("[AppBootstrap] ĳ�� �ε� ����");
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
            Debug.Log("[AppBootstrap] ĳ�� �ε� �Ϸ�");
        } 
        IEnumerator Wrap(IEnumerator routine, Action onDone)
        {
            yield return routine;
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
                    GameLogger.Warn($"[AppBootstrap] [Auth Refresh] ����: {res.Message}");
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