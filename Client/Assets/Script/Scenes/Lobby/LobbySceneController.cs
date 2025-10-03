using Contracts.Protos;
using Game.Core;
using Game.Lobby;
using Game.Network;
using Game.UICommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scenes.Lobby
{
    public class LobbySceneController : MonoBehaviour
    {
        [Header("UI Refs")]
        public UserProfileIUI ProfileUI;
        public CurrencyUI CurrencyUI;
        public InventoryUI ItemTypeUI;

        [Header("Optional")]
        public LoadingSpinner Spinner;    // 없으면 무시
        public Popup Popup;               // 없으면 무시
        public ProtoHttpClient Http;      // 비워도 자동 탐색

        private void Awake()
        {
            if (Http == null) Http = ProtoHttpClient.Instance ?? FindObjectOfType<ProtoHttpClient>();
        }

        private void Start()
        {
            StartCoroutine(CoInit());
        }

        private IEnumerator CoInit()
        {
            Spinner?.Show(true);

            UserProfilePb profile = null;

            // /api/pb/me/profile 호출
            yield return Http.Get(ApiRoutes.MeProfile, UserProfilePb.Parser, (ApiResult<UserProfilePb> res) =>
            {
                if (!res.Ok)
                {
                    Popup?.Show($"프로필 불러오기 실패: {res.Message}");
                    return;
                }
                profile = res.Data;
            });

            if (profile != null)
            {
                LobbySet(profile);
            }

            Spinner?.Show(false);
        }

        private void LobbySet(UserProfilePb profile)
        {
            ProfileUI?.Set(profile);
            CurrencyUI?.Set(profile);
        }

    }

}