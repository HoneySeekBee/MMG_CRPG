using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ItemTypeToggleUI : MonoBehaviour
{
    public Toggle itemTypeToggle;
    public TMP_Text nameText;
    public GameObject bgObject;

    private void Awake()
    {
        itemTypeToggle = this.GetComponent<Toggle>();
    }
    public void Set(string name, ToggleGroup group) 
    {
        nameText.text = name;
        itemTypeToggle.group = group;
        itemTypeToggle.onValueChanged.AddListener(OnOff);
    }
    public void OnOff(bool active = false)
    {
        if (bgObject != null)
            bgObject.SetActive(active);
    }
}
