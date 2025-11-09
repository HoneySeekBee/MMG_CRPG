using Client.Systems;
using Contracts.CharacterModel;
using Contracts.Protos;
using Contracts.UserParty;
using Game.Core;
using Game.Data;
using Game.Network;
using Game.UICommon;
using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static System.Net.WebRequestMethods;
using static UnityEditor.AddressableAssets.Build.Layout.BuildLayout;

public class UserPartyNetwork
{ 
    private readonly int _userId;
    public ProtoHttpClient Http;
    private Popup Popup;
    public UserPartyNetwork(Popup popup = null)
    { 
        _userId = GameState.Instance.CurrentUser.UserId;
        Http = AppBootstrap.Instance.Http;
        Popup = popup;
    }
    // 1) 가져오기
    public IEnumerator GetPartyAsync(int battleId)
    { 
        yield return Http.Get(ApiRoutes.UserPartyGet(_userId, battleId), GetUserPartyResponsePb.Parser, (ApiResult<GetUserPartyResponsePb> res) =>
        {
            if (!res.Ok)
            {
                Debug.LogError($"[UserPartyResponsePb] 요청 실패: {res.Message}");
                Popup?.Show($"유저 파티 불러오기 실패: {res.Message}");
                return;
            }

            if (res.Data == null || res.Data.Party == null)
            {
                Debug.LogWarning("[UserPartyResponsePb] Data가 null입니다 (서버 응답 없음)");
                return;
            }

            // 진행 데이터가 없는 경우 = 신규 유저
            if (res.Data.Party.Slots.Count == 0)
            {
                Debug.Log("[UserStageProgress] 파티 데이터가 없음 (신규 유저)");
            }

            // 정상 데이터 있을 때만 싱크
            Debug.Log($"[UserPartyResponsePb] {res.Data.Party.Slots.Count}이 불러짐");
            GameState.Instance.CurrentUser?.SyncUserParty(battleId, res.Data.Party.Slots);
        });
    }

    // 2) 저장(완료 시 한 번에)
    public void SaveParty(long partyId, IEnumerable<(int slotId, int? userCharacterId)> pairs)
    {
        // 1) 요청 만들기
        var req = new BulkAssignRequestPb
        {
            PartyId = partyId
        };

        foreach (var (slotId, userCharacterId) in pairs)
        {
            req.Pairs.Add(new BulkAssignRequestPb.Types.AssignPair
            {
                SlotId = slotId,
                UserCharacterId = userCharacterId   
            });
        }

        // 2) URL 맞추기 
        string url = ApiRoutes.UserPartyBulkAssign; 
        Debug.Log($"파티 저장 : {url}");

        // 3) 서버는 NoContent() 
        AppBootstrap.Instance.StartCoroutine(AppBootstrap.Instance.Http.Put(url, req, Empty.Parser, OnSavePartyResponse));
    }

    // 서버 응답 처리
    private void OnSavePartyResponse(ApiResult<Empty> res)
    {
        if (!res.Ok)
        {
            Debug.LogError("[UserParty] 파티 저장 실패: " + res.Message);
            Popup?.Show($"파티 저장 실패: {res.Message}");
            return;
        }

        Debug.Log("[UserParty] 파티 저장 성공"); 
    }
}
