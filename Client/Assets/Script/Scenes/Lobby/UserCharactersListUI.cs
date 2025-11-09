using Contracts.Protos;
using Game.Data;
using Lobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserCharactersListUI : UIPopup
{
    public static UserCharactersListUI Instance { get; private set; }

    [Header("List UI")]
    [SerializeField] private RectTransform iconParent;
    [SerializeField] private UserCharacterUI prefab;
    
    [Header("Data / Controller")]
    [SerializeField] private UserCharacterListController controller;
    private readonly List<UserCharacterUI> uiPool = new();

    [Header("Detail (optional)")]
    public UserCharacterDeatailUI UserCharacterDeatailScript;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (controller != null)
            controller.OnListChanged += UpdateUI;
    }
    private void OnEnable()
    {
        // 켜질 때 최신 리스트 요청
        if (controller != null)
            controller.RefreshList();
    }
    private void OnDisable()
    {
        // 팝업 닫힐 때 디테일도 닫아주기
        if (UserCharacterDeatailScript != null)
            UserCharacterDeatailScript.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (controller != null)
            controller.OnListChanged -= UpdateUI;

        if (Instance == this)
            Instance = null;
    }

    private void UpdateUI(List<UserCharacterSummaryPb> characters)
    {
        for (int i = 0; i < uiPool.Count; i++)
            uiPool[i].gameObject.SetActive(false);

        if (characters == null || characters.Count == 0)
            return;
        for (int i = 0; i < characters.Count; i++)
        {
            UserCharacterUI ui;
            if (i < uiPool.Count)
            {
                ui = uiPool[i];
            }
            else
            {
                ui = Instantiate(prefab, iconParent);
                uiPool.Add(ui);
            }

            var summary = characters[i];
            ui.Set(summary, (data) =>
            {
                var detailUI = UserCharactersListUI.Instance.UserCharacterDeatailScript;
                detailUI.gameObject.SetActive(true);
                detailUI.Set(data);
            });
            ui.gameObject.SetActive(true);

            // 디테일창이 있으면 클릭 콜백 넘겨주기 (UserCharacterUI에 이런 메서드가 있다고 가정)
            if (UserCharacterDeatailScript != null)
            {
                ui.thisBtn.onClick.AddListener(()=>
                {
                    UserCharacterDeatailScript.gameObject.SetActive(true);
                    UserCharacterDeatailScript.Set(summary);
                });
            }
        }
    }


}
