using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemImage; 

    public void Set(Sprite itemSprite)
    {
        itemImage.sprite = itemSprite;
    }
}
