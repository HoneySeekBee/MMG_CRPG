using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPopup : MonoBehaviour
{
    [SerializeField] private GameObject LoadingObj;
    [SerializeField] private GameObject SpeechObj;
    [SerializeField] private TMP_Text speechText; 
    public void Show(string speech = "")
    {
        LoadingObj.SetActive(true);
        if (speech == "")
            SpeechObj.SetActive(false);
        else
        {
            SpeechObj.SetActive(true);
            speechText.text = speech;
        }
    }
    public void UnShow()
    {
        LoadingObj.SetActive(false);
    }
}
