using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (fadeImage == null)
            fadeImage = GetComponent<Image>();
    }

    // 시작 시 페이드인 하고 싶으면 Start에서 호출 가능
    private void Start()
    {
        //StartCoroutine(FadeIn());
    }
    private Coroutine CoFade;

    public void Start_FadeIn()
    {
        if(CoFade != null)
        {
            StopCoroutine(CoFade);
            CoFade = null;
        }
        CoFade = StartCoroutine(FadeIn());
    }
    public void Start_Fadeout()
    {
        if (CoFade != null)
        {
            StopCoroutine(CoFade);
            CoFade = null;
        }
        CoFade = StartCoroutine(FadeOut());
    }
    public void Direct_FadeOut()
    {
        if(fadeImage.gameObject.activeSelf == false)
            fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
    }
    public IEnumerator FadeIn()
    {
        float time = 0f;
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color; 
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, time / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        fadeImage.gameObject.SetActive(false);
        CoFade = null;
    }

    public IEnumerator FadeOut()
    {
        float time = 0f;
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        fadeImage.gameObject.SetActive(true);
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
            fadeImage.color = color;
            yield return null;
        } 
        CoFade = null;
    }
}
