using Client.Systems;
using Game.Core;
using Game.Network;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingNetwork
{
    private readonly ProtoHttpClient _http;

    public PingNetwork()
    {
        _http = AppBootstrap.Instance.Http;
    }

    // /api/ping POST
    public IEnumerator SendPing(Action<bool> onDone = null)
    {
        string url = ApiRoutes.Ping;

        Debug.Log($"[PingScheduler] Ping 보냄 {url}");
        // 빈 protobuf 전송 (Empty message)
        yield return _http.Post(url, new Empty(), Empty.Parser, (ApiResult<Empty> res) =>
        {
            Debug.Log($"[PingScheduler] Ping 보내기 결과 {res.Ok} : {res.Message}");
            if (!res.Ok)
            {
                Debug.LogWarning($"[PingNetwork] Ping 실패: {res.Message}");
                onDone?.Invoke(false);
                return;
            }
            onDone?.Invoke(true);
        });
    }
}
