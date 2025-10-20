using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Contracts.Protos;
using WebServer.Protos;

public class UserCharacterUI : MonoBehaviour
{
    public Image FactionBGImage;
    public Image PortraitsImage; 
    public Image ElementIconImage;
    public Image RoleIconImage; 
    public TMP_Text NameText; 

    public void Set(UserCharacterSummaryPb characterData)
    {
        CharacterSummaryPb ThisCharacter = CharacterCache.Instance.SummaryById[characterData.CharacterId];
        MasterDataCache MasterData = MasterDataCache.Instance;
        // FactionBG는 아이콘이 아니라 그냥 색상만 참조하면 된다. 
        PortraitsImage.sprite = MasterData.PortraitSprites[ThisCharacter.Id];
        ElementIconImage.sprite = MasterData.IconSprites[MasterData.ElementList[ThisCharacter.ElementId].IconId];
        RoleIconImage.sprite = MasterData.IconSprites[MasterData.RoleList[ThisCharacter.RoleId].IconId];
        NameText.text = ThisCharacter.Name;
    }
}
