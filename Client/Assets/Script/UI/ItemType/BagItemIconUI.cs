using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Contracts.Protos;
public class BagItemIconUI : MonoBehaviour
{
    public ItemMessage ItemData;
    public TMP_Text countText;
    public Image IconImage;

    public void Set(ItemMessage _itemData, Sprite iconSprite, int count)
    {
        ItemData = _itemData;
        IconImage.sprite = iconSprite;
        countText.text = count.ToString();
    }

}
