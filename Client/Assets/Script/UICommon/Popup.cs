using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game.UICommon
{
    public class Popup : MonoBehaviour
    {
        public GameObject Root;
        public TMP_Text MessageText;
        public void Show(string msg) { if (MessageText) MessageText.text = msg; if (Root != null) Root.SetActive(true); }
        public void Hide() { if (Root != null) Root.SetActive(false); }
    }
}