using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebServer.Protos;

public class UserCharacterEquipUI : MonoBehaviour
{
    [SerializeField] private EquipSlotUI[] EquipSlots;
    [SerializeField] private Image ProfileImage;

    private void OnEnable()
    {
        foreach (var slot in EquipSlots)
        {
            slot.Set();
        }
        UserCharacterSummaryPb characterSummary = UserCharacterDeatailUI.Instance.status;
        CharacterDetailPb characterDetail = CharacterCache.Instance.DetailById[characterSummary.CharacterId]; 
        ProfileImage.sprite = MasterDataCache.Instance.PortraitSprites[characterDetail.PortraitId ?? 0];
    }

}
