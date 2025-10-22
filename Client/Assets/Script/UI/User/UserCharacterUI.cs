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

    public Button thisBtn;

    private void Awake()
    {
        thisBtn = GetComponent<Button>();
    }

    public void Set(UserCharacterSummaryPb characterData)
    {
        CharacterDetailPb ThisCharacter = CharacterCache.Instance.DetailById[characterData.CharacterId];
        MasterDataCache MasterData = MasterDataCache.Instance;
        // FactionBG는 아이콘이 아니라 그냥 색상만 참조하면 된다. 

        PortraitsImage.sprite = MasterData.PortraitSprites[ThisCharacter.PortraitId ?? 0];
        ElementIconImage.sprite = MasterData.IconSprites[MasterData.ElementDictionary[ThisCharacter.ElementId].IconId];
        RoleIconImage.sprite = MasterData.IconSprites[MasterData.RoleDictionary[ThisCharacter.RoleId].IconId];
        NameText.text = ThisCharacter.Name;

        thisBtn.onClick.RemoveAllListeners();
        thisBtn.onClick.AddListener(() =>
        {
            UserCharactersListUI.Instance.UserCharacterDeatailScript.gameObject.SetActive(true);
            UserCharactersListUI.Instance.UserCharacterDeatailScript.Set(characterData);
        });
    }


}
