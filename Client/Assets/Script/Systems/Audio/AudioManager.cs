using System.Collections;
using System.Collections.Generic;
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
    public async void PlayBGM(string key)
    {
        var clip = await AddressableManager.Instance.LoadAsync<AudioClip>(key);
        bgmPlayer.Play(clip);
    }
    public void PlaySFX(AudioClip clip)
    {
        sfxPlayer.Play(clip);
    }
    public async void PlaySFX(string key)
    {
        var clip = await AddressableManager.Instance.LoadAsync<AudioClip>(key);
        sfxPlayer.Play(clip);
    }
}
