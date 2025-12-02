using Contracts.Protos;
using Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using WebServer.Protos;

public class GachaResultPopup : UIPopup
{
    [Header("GachaAnimation")]
    [SerializeField] private GameObject GachaAnimationObj;
    [SerializeField] private Button GachaAnimationSkipBtn;
    private GachaDrawResultPb currentResult;
    [Header("GachaReulst")]
    [SerializeField] private ObjectPool pool;
    [SerializeField] private RectTransform GachaNotice_Rect;
    private readonly List<GachaNoticeUI> activeNotices = new();
    [SerializeField] private Button BackBtn; 

    private void OnDisable()
    {
        // 생성된 모든 배너 UI 반환
        GachaResult_Init();
        currentResult = null;
    }

    public void Set(Action Fade, GachaDrawResultPb result)
    {
        // (1) pool에 있던 애들 비활성화 
        GachaResult_Init(); 
        currentResult = result;
        GachaAnimationSkipBtn.onClick.RemoveAllListeners();
        StartCoroutine(GachaAnimation(Fade, 2));
    }
    private void GachaResult_Init()
    {
        foreach (var banner in activeNotices)
            pool.Return(banner.gameObject);
        activeNotices.Clear();
    }

    private IEnumerator GachaAnimation(Action fade,int skipTime)
    {
        fade.Invoke();
        // 애니메이션 연출 시작 
        yield return new WaitForSeconds(skipTime);
        GachaAnimationSkipBtn.onClick.AddListener(Disable_GachaAnimation);
    }
    private void Disable_GachaAnimation()
    {
        // 가챠씬 비활성화 

        Show_GachaResult();
    }

    public void Show_GachaResult()
    {
        // (2) 가챠 결과에 맞추어 보여주기 
        BackBtn.onClick.RemoveAllListeners();
        foreach (var gacha in currentResult.Items)
        {
            GameObject go = pool.Get();
            go.transform.SetParent(GachaNotice_Rect, false);
            GachaNoticeUI ui = go.GetComponent<GachaNoticeUI>();
            CharacterDetailPb characterData = CharacterCache.Instance.DetailById[gacha.CharacterId];
            int characterPortraitId = characterData.PortraitId ?? 0;
            Sprite characterPortraits = MasterDataCache.Instance.PortraitSprites[characterPortraitId];
            int characterStar = MasterDataCache.Instance.RarityDictionary[characterData.RarityId].Stars;
            ui.Set(gacha.IsNew, characterPortraits, characterStar, gacha.ShardAmount);
            activeNotices.Add(ui);
        } 
        BackBtn.onClick.AddListener(() => LobbyRootController.Instance.Show("GachaShop"));
    }
}
