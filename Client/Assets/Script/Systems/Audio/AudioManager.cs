using Game.Logging;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private BGMPlayer bgmPlayer;
    [SerializeField] private SFXPlayer sfxPlayer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayBGM(AudioClip clip)
    {
        bgmPlayer.Play(clip);
    }
    public async Task PlayBGM(string key)
    {
        try
        {
            var clip = await AddressableManager.Instance.LoadAsync<AudioClip>(key);
            bgmPlayer.Play(clip);
        }
        catch (Exception e)
        {
            GameLogger.Error($"[AudioManager] PlayBGM failed: {e.Message}");
        }
    }
    public void PlaySFX(AudioClip clip)
    {
        sfxPlayer.Play(clip);
    }
    public async Task PlaySFX(string key)
    {
        try
        {
            var clip = await AddressableManager.Instance.LoadAsync<AudioClip>(key);
            sfxPlayer.Play(clip);
        }
        catch (Exception e)
        {
            GameLogger.Error($"[AudioManager] PlaySFX failed: {e.Message}");
        }
    }
}
