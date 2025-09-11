using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Game.UICommon
{
    public class LoadingSpinner : MonoBehaviour
    {
        public GameObject Root;
        public void Show(bool show) { if (Root != null) Root.SetActive(show); }
    }
}