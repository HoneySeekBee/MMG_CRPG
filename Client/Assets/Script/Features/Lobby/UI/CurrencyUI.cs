using UnityEngine;
using TMPro;
using Contracts.Protos;
using Game.Data;

namespace Game.Lobby
{
    public class CurrencyUI : MonoBehaviour
    {
        public TMP_Text TokenText;
        public TMP_Text GoldTextt;
        public TMP_Text GemText;

        private void OnEnable()
        {
            GameState.Instance.OnCurrencyChanged += Set;
            var profile = GameState.Instance.CurrentUser?.UserProfilePb;
            if (profile != null) Set(profile);
        }

        private void OnDisable()
        {
            if (GameState.Instance != null)
                GameState.Instance.OnCurrencyChanged -= Set;
        }

        public void Set(UserProfilePb p)
        {
            TokenText.text = p.Token.ToString();
            GoldTextt.text = p.Gold.ToString();
            GemText.text = p.Gem.ToString();
        }
    }
}