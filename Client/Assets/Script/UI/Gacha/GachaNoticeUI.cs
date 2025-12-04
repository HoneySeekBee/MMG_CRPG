using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Contracts.Protos;
using UnityEngine.SocialPlatforms.Impl;
public class GachaNoticeUI : MonoBehaviour
{
    [Header("GachaResult")]
    [SerializeField] private Image CharacterImage;
    [SerializeField] private TMP_Text overlapCount;
    private Coroutine coroutine;
    [SerializeField] private Image[] CharacterStars;
    public void Set(bool isNew, Sprite characterImage, int starCount = 3, int getOverlap = 10)
    {
        CharacterImage.gameObject.SetActive(true);
        overlapCount.text = null;

        if(isNew != false)
        {
            coroutine = StartCoroutine(ShowOverlap(getOverlap));
        }
        CharacterImage.sprite = characterImage;
        Sprite yellowStar = UIImageCache.Instance.Get(UIImageCache.YellowStarKey);
        Sprite grayStar = UIImageCache.Instance.Get(UIImageCache.GrayStarKey);
        for (int i = 0; i < CharacterStars.Length; i++)
        {
            CharacterStars[i].sprite = (i <= starCount - 1) ? yellowStar : grayStar;
        }
    }
    private void OnDisable()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        overlapCount.text = null;
    }
    private IEnumerator ShowOverlap(int get)
    {
        yield return new WaitForSeconds(1);
        CharacterImage.gameObject.SetActive(false);
        overlapCount.text = $"+{get}";
        coroutine = null;
    }
}
