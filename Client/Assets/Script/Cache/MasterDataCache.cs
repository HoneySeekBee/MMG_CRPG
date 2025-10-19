using Contracts.Assets;
using Contracts.Protos;
using Game.Core;
using Game.MasterData;
using Game.Network;
using Game.UICommon;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine; 
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;

public class MasterDataCache : MonoBehaviour
{
    public static MasterDataCache Instance { get; private set; }

    [Header("MasterData - Rarity, Eelement, Role, Faction")]
    public List<RarityMessage> RarityList;
    public List<ElementMessage> ElementList;
    public List<RoleMessage> RoleList;
    public List<FactionMessage> FactionList;

    [Header("Icons / Portraits")]
    public Dictionary<int, Sprite> IconSprites = new();
    public Dictionary<int, Sprite> PortraitSprites = new(); 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator CoLoadMasterData(  ProtoHttpClient http, Popup popup)
    { 
        yield return http.Get(ApiRoutes.MasterData, MasterDataBundle.Parser,
       (ApiResult<MasterDataBundle> res) =>
       {
           if (!res.Ok)
           {
               popup?.Show($"마스터데이터 불러오기 실패: {res.Message}");
               return;
           }

           var data = res.Data;

           RarityList = data.Rarities.ToList();
           ElementList = data.Elements.ToList();
           RoleList = data.Roles.ToList();
           FactionList = data.Factions.ToList();

           Debug.Log($"[MasterDataCache] Loaded: " +
                     $"Rarity={RarityList.Count}, " +
                     $"Element={ElementList.Count}, " +
                     $"Role={RoleList.Count}, " +
                     $"Faction={FactionList.Count}");
       });

        
        yield return StartCoroutine(CoLoadIcons(http, popup));
        yield return StartCoroutine(CoLoadPortraits(http, popup));
          

    }
    #region Icon / Portrait
    public IEnumerator CoLoadIcons(ProtoHttpClient http, Popup popup)
    {
        yield return http.Get(ApiRoutes.Icons, ListIconsResponse.Parser, (ApiResult<ListIconsResponse> res) =>
        {
            if (!res.Ok)
            {
                popup?.Show($"아이콘 불러오기 실패: {res.Message}");
                return;
            }

            StartCoroutine(CoDownloadIcons(res.Data.Icons));
        });
    }
    private IEnumerator CoDownloadIcons(IEnumerable<IconMessage> list)
    {
        foreach (var item in list)
        {
            using var req = UnityWebRequestTexture.GetTexture(item.Url);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"아이콘 다운로드 실패: {item.Url} - {req.error}");
                continue;
            }

            var tex = DownloadHandlerTexture.GetContent(req);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            IconSprites[item.IconId] = sprite;
        } 
    }

    public IEnumerator CoLoadPortraits(ProtoHttpClient http, Popup popup)
    {
        yield return http.Get(ApiRoutes.Portraits, ListPortraitsResponse.Parser, (ApiResult<ListPortraitsResponse> res) =>
        {
            if (!res.Ok)
            {
                popup?.Show($"초상화 불러오기 실패: {res.Message}");
                return;
            }

            StartCoroutine(CoDownloadPortraits(res.Data.Portraits));
        });
    }
    private IEnumerator CoDownloadPortraits(IEnumerable<PortraitMessage> list)
    {
        foreach (var item in list)
        {
            using var req = UnityWebRequestTexture.GetTexture(item.Url);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"초상화 다운로드 실패: {item.Url} - {req.error}");
                continue;
            }

            var tex = DownloadHandlerTexture.GetContent(req);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            PortraitSprites[item.PortraitId] = sprite;
        } 
    } 
    #endregion
    // --- 헬퍼 메서드 (id → 데이터 찾기) ---
    public RarityMessage GetRarity(int id) => RarityList?.FirstOrDefault(x => x.RarityId == id);
    public ElementMessage GetElement(int id) => ElementList?.FirstOrDefault(x => x.ElementId == id);
    public RoleMessage GetRole(int id) => RoleList?.FirstOrDefault(x => x.RoleId == id);
    public FactionMessage GetFaction(int id) => FactionList?.FirstOrDefault(x => x.FactionId == id);
}
