using Client.Systems;
using Combat;
using Game.Core;
using Game.Data;
using Game.Network;
using Game.UICommon;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using UnityEngine;
public class CombatNetwork
{
    private readonly int _userId;
    public ProtoHttpClient Http;
    private Popup _popup;
    public CombatNetwork(Popup popup = null)
    {
        _userId = GameState.Instance.CurrentUser.UserId;
        Http = AppBootstrap.Instance.Http;
        _popup = popup;
    }

    // [1] 전투 입장
    public IEnumerator StartCombatAsync(
      int stageId,
      long battleId,
      Action<ApiResult<StartCombatResponsePb>> onDone)
    {
        var req = new StartCombatRequestPb
        {
            StageId = stageId,
            UserId = _userId,
            BattleId = (int)battleId
        };

        string url = ApiRoutes.CombatStart;
        // 예: public const string CombatStart = "/api/pb/combat/start";

        Debug.Log($"[CombatNetwork] StartCombat: {url}, stage={stageId}, formation={battleId}");

        yield return Http.Post(url, req, StartCombatResponsePb.Parser, (ApiResult<StartCombatResponsePb> res) =>
        {
            if (!res.Ok)
            {
                Debug.LogError($"[CombatNetwork] StartCombat 실패: {res.Message}");
                _popup?.Show($"전투 시작 실패: {res.Message}");
            }

            onDone?.Invoke(res);
        });
    }

    // [2] 전투 명령 ( 스킬 사용 )
    public void SendCommand(
    long combatId,
    long actorId,
    long skillId,
    long? targetActorId = null,
    Action<bool> onDone = null)
    {
        var cmd = new CombatCommandPb
        {
            ActorId = actorId,
            SkillId = skillId
        };

        if (targetActorId.HasValue)
        {
            cmd.TargetActorId = targetActorId.Value;
        }

        string url = ApiRoutes.CombatCommand(combatId);
        // 예: public static string CombatCommand(long combatId) => $"/api/pb/combat/{combatId}/command";

        Debug.Log($"[CombatNetwork] SendCommand: {url}, actor={actorId}, skill={skillId}, target={targetActorId}");

        AppBootstrap.Instance.StartCoroutine(
            Http.Post(url, cmd, Empty.Parser, resp =>
            {
                if (!resp.Ok)
                {
                    Debug.LogError($"[CombatNetwork] Command 실패: {resp.Message}");
                    _popup?.Show($"스킬 사용 실패: {resp.Message}");
                    onDone?.Invoke(false);
                    return;
                }

                onDone?.Invoke(true);
            })
        );
    }

    // [3] 로그 조회
    public IEnumerator GetLogAsync(
      long combatId,
      string cursor,
      int size,
      Action<ApiResult<CombatLogPagePb>> onDone)
    {
        string url = ApiRoutes.CombatLog(combatId, cursor, size);
        // 예: public static string CombatLog(long combatId, string cursor, int size)
        //     => $"/api/pb/combat/{combatId}/log?cursor={cursor}&size={size}";

        Debug.Log($"[CombatNetwork] GetLog: {url}");

        yield return Http.Get(url, CombatLogPagePb.Parser, (ApiResult<CombatLogPagePb> res) =>
        {
            if (!res.Ok)
            {
                Debug.LogError($"[CombatNetwork] GetLog 실패: {res.Message}");
                // 로그 폴링 실패는 팝업은 선택 사항
            }

            onDone?.Invoke(res);
        });
    }

    // [4] 요약 조회 
    public IEnumerator GetSummaryAsync(
      long combatId,
      Action<ApiResult<CombatLogSummaryPb>> onDone)
    {
        string url = ApiRoutes.CombatSummary(combatId);
        // 예: public static string CombatSummary(long combatId)
        //     => $"/api/pb/combat/{combatId}/summary";

        Debug.Log($"[CombatNetwork] GetSummary: {url}");

        yield return Http.Get(url, CombatLogSummaryPb.Parser, (ApiResult<CombatLogSummaryPb> res) =>
        {
            if (!res.Ok)
            {
                Debug.LogError($"[CombatNetwork] GetSummary 실패: {res.Message}");
                _popup?.Show($"전투 결과 불러오기 실패: {res.Message}");
            }

            onDone?.Invoke(res);
        });
    }

}
