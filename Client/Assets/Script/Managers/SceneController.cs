using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Managers
{
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance { get; private set; }

        private string _current;             // 현재 활성 씬 이름 (AppPersistent 제외)
        private const string PersistentName = "AppPersistent";
        public const string PartySetupSceneName = "PartySetupScene";
        public const string GachaSceneName = "GachaAnimationScene";
        public const string MapSceneName = "Battle";
        private readonly HashSet<string> _loadedAdditives = new HashSet<string>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // 기존 사용 부분
        public void Go(string sceneName)
        {
            Debug.Log($"[SceneController] Go: {sceneName}");
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            _current = sceneName;
        }

        // 신규 사용 부분 
        public IEnumerator GoAsync(string sceneName)
        {
            if (_current == sceneName)
            {
                Debug.Log($"[SceneController] 이미 활성: {sceneName}");
                yield break;
            }

            Debug.Log($"[SceneController] 전환: {_current ?? "None"} → {sceneName}");

            // 새 씬 Additive 로드
            var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!load.isDone) yield return null;

            var newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid())
                SceneManager.SetActiveScene(newScene);

            // 이전 씬 언로드 (AppPersistent 제외)
            if (!string.IsNullOrEmpty(_current) && _current != PersistentName)
            {
                var unload = SceneManager.UnloadSceneAsync(_current);
                while (unload != null && !unload.isDone) yield return null;
            }

            _current = sceneName;
            Debug.Log($"[SceneController] 전환 완료: {_current}");
        }
        public IEnumerator LoadAdditiveAsync(string sceneName, bool setActive = false)
        { 
            if (_loadedAdditives.Contains(sceneName))
            {
                Debug.Log($"[SceneController] Additive 이미 로드됨: {sceneName}");
                yield break;
            }

            Debug.Log($"[SceneController] Additive 로드: {sceneName}");
            var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!load.isDone) yield return null;

            var scene = SceneManager.GetSceneByName(sceneName);
            if (setActive && scene.IsValid())
            {
                SceneManager.SetActiveScene(scene);
            }

            _loadedAdditives.Add(sceneName);
        }

        // 보조(콘텐츠) 씬 언로드
        public IEnumerator UnloadAdditiveAsync(string sceneName)
        {
            if (!_loadedAdditives.Contains(sceneName))
            {
                Debug.Log($"[SceneController] Additive 언로드 요청했지만 목록에 없음: {sceneName}");
                yield break;
            }

            Debug.Log($"[SceneController] Additive 언로드: {sceneName}");
            var unload = SceneManager.UnloadSceneAsync(sceneName);
            while (unload != null && !unload.isDone) yield return null;

            _loadedAdditives.Remove(sceneName);
        }

    }
}