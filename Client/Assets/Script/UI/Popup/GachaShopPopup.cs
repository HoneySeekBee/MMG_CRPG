using Contracts.Protos;
using Game.Data;
using Game.Lobby;
using Game.Managers;
using Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private Button Home_Btn;

    private void OnDisable()
    {
        ReturnAllBanners();
    }

    public void Set(Action fadeIn, GachaCatalogPb gachaCatalog)
    {
        Debug.Log($"배너 갯수 {gachaCatalog.Banners.Count}");
        Set_Banner(gachaCatalog);
        Set_Currency();
        StartCoroutine(LoadGachaScnee(fadeIn));
        Home_Btn.onClick.RemoveAllListeners();
        Home_Btn.onClick.AddListener(() =>
        {
            GachaAnimationManager.Instance.DisableGacha();
            LobbyRootController.Instance.Show("Main");
        });
    }
    private void ReturnAllBanners()
    {
        foreach (var banner in activeBanners)
        {
            if (banner != null)
            {
                banner.transform.SetParent(null, false);
                pool.Return(banner.gameObject);
            }
        }

        activeBanners.Clear();
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
        if (activeBanners.Count > 0)
        {
            var firstToggle = activeBanners[0].GetComponent<Toggle>();
            firstToggle.isOn = true; // 자동 선택
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
        BannerBG.sprite = bannerImage; // 이거는 나중에 해주자. ( 현재 해당 이미지 투명도 0으로 해놓았다. ) 

        Gacha_One.onClick.RemoveAllListeners();
        Gacha_Ten.onClick.RemoveAllListeners();

        GachaNetwork network = NetworkManager.Instance.GachaNetwork;
        Gacha_One.onClick.AddListener(() =>
        {
            Debug.Log($"뽑기 1회 : {data.Title}");
            StartCoroutine(network.DrawAsync(data.Key, 1, (res) =>
            {
                if (!res.Ok)
                {
                    Debug.LogError($"Draw 실패: {res.Message}");
                    return;
                }

                LobbyRootController.Instance.GachaResult(res.Data);
                LobbyRootController.Instance.Show("GachaResult");
            }));
        });

        Gacha_Ten.onClick.AddListener(() =>
        {
            Debug.Log($"뽑기 10회 : {data.Title}");
            StartCoroutine(network.DrawAsync(data.Key, 10, (res) =>
            {
                if (!res.Ok)
                {
                    Debug.LogError($"Draw 실패: {res.Message}");
                    return;
                }

                LobbyRootController.Instance.GachaResult(res.Data);
                LobbyRootController.Instance.Show("GachaResult");
            }));
        });
        Gacha_Ten.onClick.AddListener(() => Console.WriteLine($"뽑기 10회 : {data.Title}"));
    }

}
