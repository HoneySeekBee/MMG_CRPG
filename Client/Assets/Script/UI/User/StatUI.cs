using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MMG_CRPG.UI
{
    public enum StatType { HP, ATK, DEF, SPD, CRI_RATE, CRI_DAMAGE }
    
    public class StatUI : MonoBehaviour
    {
        public StatType type;
        [SerializeField] private TMP_Text text;

        public void SetValue(int value)
        {
            text.text = value.ToString();
        }
    }
}
