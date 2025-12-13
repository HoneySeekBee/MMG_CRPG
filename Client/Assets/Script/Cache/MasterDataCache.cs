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
using System.Security.Cryptography;
using UnityEngine; 
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;

public class MasterDataCache : MonoBehaviour
{
    public static MasterDataCache Instance { get; private set; }

    [Header("MasterData - Rarity, Eelement, Role, Faction")] 

    public Dictionary<int, RarityMessage> RarityDictionary = new();
    public Dictionary<int, ElementMessage> ElementDictionary = new();
    public Dictionary<int, RoleMessage> RoleDictionary = new();
    public Dictionary<int, FactionMessage> FactionDictionary = new();

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
           RarityDictionary = data.Rarities.ToDictionary(r => r.RarityId);
           ElementDictionary = data.Elements.ToDictionary(e => e.ElementId);
           RoleDictionary = data.Roles.ToDictionary(r => r.RoleId);
           FactionDictionary = data.Factions.ToDictionary(f => f.FactionId);

           Debug.Log($"[MasterDataCache] Loaded: " +
                     $"Rarity={data.Rarities.Count}, " +
                     $"Element={data.Elements.Count}, " +
                     $"Role={data.Roles.Count}, " +
                     $"Faction={data.Factions.Count}");
       });

        bool isLoadIcon = false, isLoadPortraits = false;

        StartCoroutine(CoLoadIcons(http, popup, () => isLoadIcon = true));
        StartCoroutine(CoLoadPortraits (http, popup, () => isLoadPortraits = true));

        while (isLoadIcon == false || isLoadPortraits == false)
            yield return null; 
    }
    #region Icon / Portrait
    public IEnumerator CoLoadIcons(ProtoHttpClient http, Popup popup, System.Action onDone)
    {
        yield return http.Get(ApiRoutes.Icons, ListIconsResponse.Parser, (ApiResult<ListIconsResponse> res) =>
        {
            if (!res.Ok)
            {
                popup?.Show($"아이콘 불러오기 실패: {res.Message}");
                return;
            }

            StartCoroutine(CoDownloadIcons(res.Data.Icons, onDone));
        });
    }
    private IEnumerator CoDownloadIcons(IEnumerable<IconMessage> list, System.Action onDone)
    {
        foreach (var item in list)
        {
            using var req = UnityWebRequestTexture.GetTexture(item.Url);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"아이콘 다운로드 실패: {item.Url} - {req.error}");
                continue;
            }

            var tex = DownloadHandlerTexture.GetContent(req);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            IconSprites[item.IconId] = sprite;
        }
        onDone.Invoke();
    }

    public IEnumerator CoLoadPortraits(ProtoHttpClient http, Popup popup, System.Action onDone)
    {
        Debug.Log("초상화 로드 시작");
        yield return http.Get(ApiRoutes.Portraits, ListPortraitsResponse.Parser, (ApiResult<ListPortraitsResponse> res) =>
        {
            if (!res.Ok)
            {
                Debug.Log($"초상화 로드 실패 {res.Message}");
                popup?.Show($"초상화 불러오기 실패: {res.Message}");
                return;
            }

            StartCoroutine(CoDownloadPortraits(res.Data.Portraits, onDone));
        });
    }
    private IEnumerator CoDownloadPortraits(IEnumerable<PortraitMessage> list, System.Action onDone)
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
            Debug.Log($"초상화 {item.PortraitId} || {sprite.name}");
        }
        onDone.Invoke(); 
    } 
    #endregion
}
