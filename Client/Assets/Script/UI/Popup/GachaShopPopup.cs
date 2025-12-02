using Contracts.Protos;
using Game.Data;
using Game.Lobby;
using Game.Managers;
using Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GachaShopPopup : UIPopup
{
    [SerializeField] private ObjectPool pool;
    [SerializeField] private Image BannerBG;

    [Header("Banner")] 
    [SerializeField] private RectTransform BannerParentRect;
    [SerializeField] private ToggleGroup BannerGroup;
    private readonly List<GachaBannerUI> activeBanners = new();

    [Header("Curreny")] 
    [SerializeField] private CurrencyUI CurrencyUI;

    [Header("Buy")]
    [SerializeField] private Button Gacha_One; 
    [SerializeField] private Button Gacha_Ten;

    private void OnDisable()
    {
        // 생성된 모든 배너 UI 반환
        foreach (var banner in activeBanners)
            pool.Return(banner.gameObject);

        activeBanners.Clear();
    }

    public void Set(Action fadeIn, GachaCatalogPb gachaCatalog)
    {
        Debug.Log($"배너 갯수 {gachaCatalog.Banners.Count}");
        Set_Banner(gachaCatalog);
        Set_Currency();
        StartCoroutine(LoadGachaScnee(fadeIn));
    }
    private IEnumerator LoadGachaScnee(Action fadeIn)
    {
        yield return SceneController.Instance.LoadAdditiveAsync(SceneController.PartySetupSceneName);
        if (fadeIn != null)
            fadeIn.Invoke();
    }
    private void Set_Banner(GachaCatalogPb gachaCatalog)
    {
        // 기존 배너 반환
        foreach (var banner in activeBanners)
            pool.Return(banner.gameObject);

        activeBanners.Clear();

        // 풀에서 가져와서 생성
        foreach (var bannerData in gachaCatalog.Banners)
        {
            GameObject go = pool.Get();
            go.transform.SetParent(BannerParentRect, false);

            GachaBannerUI ui = go.GetComponent<GachaBannerUI>();
            ui.Set(Set_Btn, bannerData, BannerGroup);

            activeBanners.Add(ui);
        }
    }
    private void Set_Currency()
    {
        var profile = GameState.Instance.CurrentUser.UserProfilePb; 
        CurrencyUI?.Set(profile);
    }

    public void Set_Btn(GachaBannerPb data, Sprite bannerImage)
    {
        BannerBG.gameObject.SetActive(true);
        BannerBG.sprite = bannerImage;

        Gacha_One.onClick.RemoveAllListeners();
        Gacha_Ten.onClick.RemoveAllListeners();
        Gacha_One.onClick.AddListener(() => Console.WriteLine($"뽑기 1회 : {data.Title}"));
        Gacha_Ten.onClick.AddListener(() => Console.WriteLine($"뽑기 10회 : {data.Title}"));
    }
}
