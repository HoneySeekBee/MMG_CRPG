using Contracts.Protos;
using Game.Data;
using Game.Network;
using Game.UICommon;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public sealed class AuthBootstrapper
{
    private readonly ProtoHttpClient _http;
    private readonly Popup _popup;


    public AuthBootstrapper(ProtoHttpClient http, Popup popup)
    {
        _http = http;
        _popup = popup;
    }

    public IEnumerator CoBootstrapAfterToken(string playerId, string access, string refresh, long serverMs)
    {
        if (!int.TryParse(playerId?.Trim(), out var pid))
        {
            Debug.Log($"playerId 형식 오류: '{playerId}'");
            _popup?.Show($"playerId 형식 오류: '{playerId}'");
            GameState.Instance.SetNeedLogin();
            _http.ClearToken();
            yield break;
        }
        // 0) 토큰 세팅  
        GameState.Instance.SetServerTimeOffset(serverMs);
        _http.SetToken(access);

        // 1) profile
        UserProfilePb profile = null;
        yield return _http.Get(ApiRoutes.MeProfile, UserProfilePb.Parser, res =>
        {
            if (!res.Ok || res.Data == null)
            {
                Debug.Log($"프로필 불러오기 실패: {res.Message}");
                _popup?.Show($"프로필 불러오기 실패: {res.Message}");
                return;
            }
            profile = res.Data;

        });
        if (profile == null) yield break;

        // 2) player bootstrap
        PlayerBootstrap boot = null;
        yield return _http.Get(ApiRoutes.PlayerBootstrap, PlayerBootstrap.Parser, res =>
        {
            if (!res.Ok || res.Data == null)
            {
                Debug.Log($"부트스트랩 실패: {res.Message}");
                _popup?.Show($"부트스트랩 실패: {res.Message}");
                return;
            }
            boot = res.Data;
        });
        if (boot == null) yield break;

        // 3) GameState에 반영 
        GameState.Instance.SaveAuth(playerId, access, refresh);
        GameState.Instance.InitUser(pid, profile.Nickname, profile.Level);
        GameState.Instance.CurrentUser ??= new UserData(pid, "Unknown", 1);
        GameState.Instance.CurrentUser.SetUserProfile(profile);
        GameState.Instance.SetNickname(boot.Nickname);
        GameState.Instance.SetCurrencies(boot.SoftCurrency, boot.HardCurrency);
        Debug.Log($"[AuthBootStrap] {GameState.Instance.CurrentUser == null}");
        // 4) 나머지 데이터들 (stage/inv/char) - [ 여기에 실패 정책 추가 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ] 
        // stage
        yield return _http.Get(ApiRoutes.UserStageProgress, MyStageProgressListPb.Parser, res =>
        {
            if (res.Ok && res.Data != null)
                GameState.Instance.CurrentUser?.SyncStageProgress(res.Data);
        });

        // inventory
        ListUserInventoryResponse invRes = null;
        yield return _http.Get(ApiRoutes.UserInventoryList(pid), ListUserInventoryResponse.Parser, res =>
        {
            if (res.Ok && res.Data != null) invRes = res.Data;
        });
        if (invRes != null)
            GameState.Instance.CurrentUser?.SyncInventory(invRes.Items);

        // characters
        UserCharacterListPb chaRes = null;
        yield return _http.Get(ApiRoutes.UserCharacterList(pid), UserCharacterListPb.Parser, res =>
        {
            if (res.Ok) chaRes = res.Data ?? new UserCharacterListPb();
        });
        if (chaRes != null)
            GameState.Instance.CurrentUser?.SyncCharacters(chaRes.Characters);
    } 
}
