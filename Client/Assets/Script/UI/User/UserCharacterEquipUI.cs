using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebServer.Protos;

namespace MMG_CRPG.UI
{
    public enum EquipType{ HEAD, CLOTHES, BOOTS, GLOVES, AMULET }
    public class UserCharacterEquipUI : MonoBehaviour
    {
        [SerializeField] private EquipSlotUI[] EquipSlots;
        [SerializeField] private Image ProfileImage;
        public EquipItemWindowUI EquipItemWindowUI;
        [HideInInspector] public CharacterDetailPb currentCharacter;

        private void OnEnable()
        {
            if(EquipItemWindowUI != null && EquipItemWindowUI.gameObject.activeSelf)
                EquipItemWindowUI.gameObject.SetActive(false);
            RefreshEquipIcon();
            UserCharacterSummaryPb characterSummary = UserCharacterDeatailUI.Instance.status;
            currentCharacter = CharacterCache.Instance.DetailById[characterSummary.CharacterId];
            ProfileImage.sprite = MasterDataCache.Instance.PortraitSprites[currentCharacter.PortraitId ?? 0];
        }

        public void RefreshEquipIcon()
        {
            foreach (var slot in EquipSlots)
            {
                slot.Set();
            }
        }
        
    }

}
