using Game.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Data
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public string PlayerId { get; private set; }
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public long ServerTimeUnixMsOffset { get; private set; } // 서버-클라 시간 오프셋

        public string Nickname { get; private set; }
        public int SoftCurrency { get; private set; }
        public int HardCurrency { get; private set; }
        public UserData CurrentUser { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
                return;
            }
            Instance = this; 
            DontDestroyOnLoad(gameObject);
        }

        public void InitUser(int userId, string nickname, int level)
        {
            CurrentUser = new UserData(userId, nickname, level);
        }
        public void LoadFromPrefs()
        {
            AccessToken = PlayerPrefs.GetString(Constants.PlayerPrefs_Token, "");
            RefreshToken = PlayerPrefs.GetString(Constants.PlayerPrefs_RefreshToken, ""); // 새 키
            PlayerId = PlayerPrefs.GetString(Constants.PlayerPrefs_PlayerId, "");

        }


        public void SaveAuth(string playerId, string accessToken, string refreshToken)
        {
            PlayerId = playerId; AccessToken = accessToken; RefreshToken = refreshToken;
            PlayerPrefs.SetString(Constants.PlayerPrefs_Token, accessToken);
            PlayerPrefs.SetString(Constants.PlayerPrefs_RefreshToken, refreshToken);
            PlayerPrefs.SetString(Constants.PlayerPrefs_PlayerId, playerId);
            PlayerPrefs.Save();
        }
        public void SetTokens(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            PlayerPrefs.SetString(Constants.PlayerPrefs_Token, accessToken);
            PlayerPrefs.SetString(Constants.PlayerPrefs_RefreshToken, refreshToken);
            PlayerPrefs.Save();
        }


        public void SetNickname(string nickname) => Nickname = nickname;
        public void SetCurrencies(int soft, int hard) { SoftCurrency = soft; HardCurrency = hard; }
        public long NowServerUnixMs()
        {
            var localMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return localMs + ServerTimeUnixMsOffset;
        }


        public void SetServerTimeOffset(long serverUnixMs)
        {
            var localMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ServerTimeUnixMsOffset = serverUnixMs - localMs;
        }

    }
}