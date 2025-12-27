using Client.Systems;
using Contracts.Protos;
using Game.Core;
using Game.Data;
using Game.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaNetwork
{
    private readonly int _userId;
    private readonly ProtoHttpClient _http;

    public GachaNetwork()
    {
        _userId = GameState.Instance.CurrentUser.UserId;
        _http = AppBootstrap.Instance.Http;
    }

    // 가차 뽑기 실행
    public IEnumerator DrawAsync(string bannerKey, int count, Action<ApiResult<GachaDrawResultPb>> onDone)
    {
        var req = new GachaDrawRequestPb
        {
            BannerKey = bannerKey,
            Count = count
        };

        string url = ApiRoutes.GachaDraw;
        // 예: public const string GachaDraw = "/api/pb/gacha/draw";

        Debug.Log($"[GachaNetwork] Draw: {url}, banner={bannerKey}, count={count}");

        yield return _http.Post(url, req, GachaDrawResultPb.Parser, (ApiResult<GachaDrawResultPb> res) =>
        {
            if (!res.Ok)
            {
                Debug.LogError($"[GachaNetwork] Draw 실패: {res.Message}");
            }

            onDone?.Invoke(res);
        });
    }

    // 활성 배너 목록 조회 
    public IEnumerator GetCatalogAsync(Action<ApiResult<GachaCatalogPb>> onDone)
    {
        string url = ApiRoutes.GachaCatalog;
        // 예: public const string GachaCatalog = "/api/pb/gacha/catalog";

        Debug.Log($"[GachaNetwork] GetCatalog: {url}");

        yield return _http.Get(url, GachaCatalogPb.Parser, (ApiResult<GachaCatalogPb> res) =>
        {
            if (!res.Ok)
            {
                Debug.LogError($"[GachaNetwork] Catalog 실패: {res.Message}");
            }

            onDone?.Invoke(res);
        });
    } 
}
