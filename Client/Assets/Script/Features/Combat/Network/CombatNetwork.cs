using Client.Systems;
using Combat;
using Game.Core;
using Game.Data;
using Game.Network;
using Game.UICommon;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using Game.Logging;
using UnityEngine;
public class CombatNetwork
{
    public static float TimeScale { get; private set; } = 1f;

    private readonly int _userId;

    // WebServer: Start / Finish
    public ProtoHttpClient Http;

    // CombatServer: Tick / Command / Speed / Log / Summary (direct, high-frequency)
    public ProtoHttpClient CombatHttp;

    private Popup _popup;
    public CombatNetwork(Popup popup = null)
    {
        _userId = GameState.Instance.CurrentUser.UserId;
        Http = AppBootstrap.Instance.Http;
        CombatHttp = AppBootstrap.Instance.CombatHttp;
        _popup = popup;
    }

    // [1] Start combat
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

        GameLogger.Info($"[CombatNetwork] StartCombat: {url}, stage={stageId}, formation={battleId}");

        yield return Http.Post(url, req, StartCombatResponsePb.Parser, (ApiResult<StartCombatResponsePb> res) =>
        {
            if (!res.Ok)
            {
                GameLogger.Error($"[CombatNetwork] StartCombat failed: {res.Message}");
                _popup?.Show($"Start combat failed: {res.Message}");
            }

            onDone?.Invoke(res);
        });
    }

    // [2] Send command (skill use)
    public void SendCommand(
    long combatId,
    long actorId,
    int skillId,
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

        GameLogger.Info($"[CombatNetwork] SendCommand: {url}, actor={actorId}, skill={skillId}, target={targetActorId}");

        AppBootstrap.Instance.StartCoroutine(
            CombatHttp.Post(url, cmd, Empty.Parser, resp =>
            {
                if (!resp.Ok)
                {
                    GameLogger.Error($"[CombatNetwork] Command failed: {resp.Message}");
                    _popup?.Show($"Skill command failed: {resp.Message}");
                    onDone?.Invoke(false);
                    return;
                }

                onDone?.Invoke(true);
            })
        );
    }

    // [3] Get combat log
    public IEnumerator GetLogAsync(
      long combatId,
      string cursor,
      int size,
      Action<ApiResult<CombatLogPagePb>> onDone)
    {
        string url = ApiRoutes.CombatLog(combatId, cursor, size);

        GameLogger.Info($"[CombatNetwork] GetLog: {url}");

        yield return CombatHttp.Get(url, CombatLogPagePb.Parser, (ApiResult<CombatLogPagePb> res) =>
        {
            if (!res.Ok)
            {
                GameLogger.Error($"[CombatNetwork] GetLog failed: {res.Message}");
                // Log failure does not need a popup
            }

            onDone?.Invoke(res);
        });
    }

    // [4] Get summary
    public IEnumerator GetSummaryAsync(
      long combatId,
      Action<ApiResult<CombatLogSummaryPb>> onDone)
    {
        string url = ApiRoutes.CombatSummary(combatId);

        GameLogger.Info($"[CombatNetwork] GetSummary: {url}");

        yield return CombatHttp.Get(url, CombatLogSummaryPb.Parser, (ApiResult<CombatLogSummaryPb> res) =>
        {
            if (!res.Ok)
            {
                GameLogger.Error($"[CombatNetwork] GetSummary failed: {res.Message}");
                _popup?.Show($"Failed to load combat summary: {res.Message}");
            }

            onDone?.Invoke(res);
        });
    }
    public IEnumerator TickAsync(long combatId, int tick, Action<ApiResult<CombatTickResponsePb>> onDone)
    {
        string url = ApiRoutes.CombatTick(combatId);

        var req = new CombatTickRequestPb
        {
            CombatId = combatId,
            Tick = tick
        };

        yield return CombatHttp.Post(url, req, CombatTickResponsePb.Parser, res =>
        {
            if (!res.Ok)
                GameLogger.Error($"[CombatNetwork] Tick failed: {res.Message}");

            onDone?.Invoke(res);
        });
    }
    public IEnumerator FinishCombatAsync(long combatId, Action<ApiResult<FinishCombatResponsePb>> onDone)
    {
        var req = new FinishCombatRequestPb
        {
            CombatId = combatId,
            UserId = _userId
        };
        string url = ApiRoutes.CombatFinish(combatId);

        GameLogger.Info($"[CombatNetwork] FinishCombat: {url}, combatId={combatId}");

        yield return Http.Post(url, req, FinishCombatResponsePb.Parser, (ApiResult<FinishCombatResponsePb> res) =>
        {
            if (!res.Ok)
            {
                GameLogger.Error($"[CombatNetwork] FinishCombat failed: {res.Message}");
                _popup?.Show($"Combat finish processing failed: {res.Message}");
            }

            onDone?.Invoke(res);
        });
    }
    public IEnumerator UseSkillAsync(long combatId, long actorId, int skillId, long? targetActorId, int skillLevel, Action<ApiResult<Empty>> onDone)
    {
        var cmd = new CombatCommandPb
        {
            ActorId = actorId,
            SkillId = skillId,
            SkillLevel = skillLevel
        };

        if (targetActorId.HasValue)
            cmd.TargetActorId = targetActorId.Value;

        string url = ApiRoutes.CombatCommand(combatId);

        GameLogger.Info($"[CombatNetwork] UseSkillAsync url={url} actor={actorId}, skill={skillId}");

        yield return CombatHttp.Post(url, cmd, Empty.Parser, (ApiResult<Empty> res) =>
        {
            if (!res.Ok)
                GameLogger.Error($"[CombatNetwork] UseSkill failed: {res.Message}");

            onDone?.Invoke(res);
        });
    }
    public IEnumerator ToggleSpeedAsync(long combatId, Action<ApiResult<ToggleSpeedResponsePb>> onDone)
    {
        var req = new ToggleSpeedRequestPb
        {
            CombatId = combatId
        };

        yield return CombatHttp.Post(
            ApiRoutes.CombatToggleSpeed(combatId),
            req,
            ToggleSpeedResponsePb.Parser,
            onDone
        );
    }
}
