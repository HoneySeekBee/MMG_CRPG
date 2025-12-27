using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Contracts.Protos;

namespace Game.Lobby
{
    public class CurrencyUI : MonoBehaviour
    {
        public TMP_Text TokenText;
        public TMP_Text GoldTextt;
        public TMP_Text GemText;

        public void Set(UserProfilePb p)
        {
            TokenText.text = p.Token.ToString();
            GoldTextt.text = p.Gold.ToString();
            GemText.text = p.Gem.ToString();
        }
    }

}