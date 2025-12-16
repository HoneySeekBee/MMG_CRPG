using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
    }

    public void Play(AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }
}
