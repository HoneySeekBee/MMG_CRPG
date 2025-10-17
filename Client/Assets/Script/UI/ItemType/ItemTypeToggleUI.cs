using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ItemTypeToggleUI : MonoBehaviour
{
    private int TypeNum;
    public Toggle itemTypeToggle;
    public TMP_Text nameText;
    public GameObject bgObject;

    private void Awake()
    {
        itemTypeToggle = this.GetComponent<Toggle>();
    }
    public void Set(string name, ToggleGroup group, int typeNum)
    {
        nameText.text = name;
        itemTypeToggle.group = group;
        itemTypeToggle.onValueChanged.AddListener(OnOff);
        itemTypeToggle.onValueChanged.AddListener(InitToggle);
        TypeNum = typeNum;
    }
    public void OnOff(bool active = false)
    {
        if (bgObject != null)
            bgObject.SetActive(active); 

    }
    public void InitToggle(bool active = false)
    {
        if (active)
        { 
            InventoryUI.Instance.Set_UserItem(TypeNum);
            InventoryUI.Instance.ItemDetailUI.gameObject.SetActive(false);
        }
    }
}
