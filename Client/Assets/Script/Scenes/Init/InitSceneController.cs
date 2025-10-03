using Game.Data;
using Game.Managers;
using Game.Network;
using Game.UICommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Contracts.Protos;

namespace Game.Scenes.Init
{
    public class InitSceneController : MonoBehaviour
    {
        public LoadingSpinner Spinner;
        public Popup Popup;
        public ProtoHttpClient Http;   // 씬에 배치
        public ApiConfig ApiConfig;    // 씬에 배치

        private IEnumerator Start()
        {
            if (Http == null) Http = FindObjectOfType<ProtoHttpClient>();
            if (SceneController.Instance == null) new GameObject("SceneController").AddComponent<Game.Managers.SceneController>();
            if (GameState.Instance == null) new GameObject("GameState").AddComponent<Game.Data.GameState>();

            Spinner?.Show(true);

            // JSON: Http.Get<StatusDto>(...)  →  PROTO: Http.Get<Status>(..., Status.Parser, ...)
            yield return Http.Get(StatusRoute(), Status.Parser, (res) =>
            {
                if (!res.Ok)
                {
                    Popup?.Show($"네트워크 오류: {res.Message}");
                    return;
                }

                // PROTO 속성명은 PascalCase (ServerUnixMs / Maintenance / ForceUpdate / Message)
                GameState.Instance.SetServerTimeOffset(res.Data.ServerUnixMs);

                if (res.Data.Maintenance)
                {
                    Popup?.Show(string.IsNullOrEmpty(res.Data.Message) ? "점검 중입니다." : res.Data.Message);
                    return;
                }

                if (res.Data.ForceUpdate)
                {
                    Popup?.Show("새 버전이 필요합니다. 스토어로 이동해주세요.");
                    return;
                }

            }); 
            
            yield return MasterDataCache.Instance.CoLoadMasterData(Http, Popup);
            yield return ItemCache.Instance.CoLoadItemData(Http, Popup);

            Spinner?.Show(false);

            SceneController.Instance.Go("Login");
        }

        private string StatusRoute() => ApiRoutes.Status; // 예: "/status"
    }
}