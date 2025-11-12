using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Contracts.Protos;
public class ItemDetailUI : MonoBehaviour
{
    public Image IconImage;
    public TMP_Text Name_Text;
    public TMP_Text Description_Text;

    public void Set(ItemMessage itemMessage)
    {
        IconImage.sprite = MasterDataCache.Instance.IconSprites[itemMessage.IconId];
        Name_Text.text = itemMessage.Name;
        Description_Text.text = itemMessage.Description;
    }
}
