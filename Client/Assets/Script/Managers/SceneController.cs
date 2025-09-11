using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Managers
{
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance { get; private set; }
        void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; DontDestroyOnLoad(gameObject); }
        public void Go(string sceneName) { SceneManager.LoadScene(sceneName); }
    }

}