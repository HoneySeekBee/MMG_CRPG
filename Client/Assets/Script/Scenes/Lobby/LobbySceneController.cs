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

        private ListItemTypesResponseMessage _itemTypes;
        private void Awake()
        {
            if (Http == null) Http = ProtoHttpClient.Instance ?? FindObjectOfType<ProtoHttpClient>();
        }

        private void Start()
        {
            StartCoroutine(CoLoadItemTypes());
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

        private IEnumerator CoLoadItemTypes()
        {
            Spinner?.Show(true);

            yield return Http.Get(ApiRoutes.ItemTypes, ListItemTypesResponseMessage.Parser,
                (ApiResult<ListItemTypesResponseMessage> res) =>
                {
                    if (!res.Ok)
                    {
                        Popup?.Show($"아이템 타입 불러오기 실패: {res.Message}");
                        return;
                    } 
                    _itemTypes = res.Data;
                });

            Debug.Log($"아이템 타입 잘 불러옴? {_itemTypes != null}");
            if (_itemTypes != null)
            {
                // 예시: Active된 것만 필터링해서 UI에 바인딩
                var activeTypes = _itemTypes.Items.Where(x => x.Active).ToList();
                Debug.Log($"아이템 타입을 불러옴 {activeTypes.Count}");
                ItemTypeUI?.Set(activeTypes);
            }

            Spinner?.Show(false);
        }
    }

}