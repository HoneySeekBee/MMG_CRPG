using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip)
    {
        source.PlayOneShot(clip);
    }
}
