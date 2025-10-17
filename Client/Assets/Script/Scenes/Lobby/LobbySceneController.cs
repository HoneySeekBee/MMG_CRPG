using Contracts.Protos;
using Game.Core;
using Game.Data;
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
            Spinner?.gameObject.SetActive(false);
            if (GameState.Instance.CurrentUser.UserProfilePb != null)
            {
                LobbySet(GameState.Instance.CurrentUser.UserProfilePb);
            } 
        } 

        private void LobbySet(UserProfilePb profile)
        {
            ProfileUI?.Set(profile);
            CurrencyUI?.Set(profile);
        }

    }

}