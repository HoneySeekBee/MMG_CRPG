using Contracts.Protos;
using Game.Core;
using Game.Network;
using Game.UICommon;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Net.WebRequestMethods;

public class ItemCache : MonoBehaviour
{
    public static ItemCache Instance { get; private set; }

    [Header("ItemType")]
    public List<ItemTypeMessage> ItemTypeList;
    private ListItemTypesResponseMessage _itemTypes;

    [Header("Items")]
    public Dictionary<long, ItemMessage> ItemDict = new();
    public Dictionary<long, int> ItemCategoryDict = new();

    private ListItemsResponse _items;
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
    public IEnumerator CoLoadItemData(  ProtoHttpClient http, Popup popup)
    {
        yield return CoLoadItemType(http, popup);

        yield return CoLoadItems(http, popup);
    }
    private IEnumerator CoLoadItemType(ProtoHttpClient http, Popup popup)
    {
        yield return http.Get(ApiRoutes.ItemTypes, ListItemTypesResponseMessage.Parser,
           (ApiResult<ListItemTypesResponseMessage> res) =>
           {
               if (!res.Ok)
               {
                   popup?.Show($"아이템 타입 불러오기 실패: {res.Message}");
                   return;
               }
               _itemTypes = res.Data;
           });

        Debug.Log($"아이템 타입 잘 불러옴? {_itemTypes != null}");
        if (_itemTypes != null)
        {
            ItemTypeList = _itemTypes.Items.Where(x => x.Active).ToList();
            Debug.Log($"아이템 타입을 불러옴 {ItemTypeList.Count}");
        }
    }

    public IEnumerator CoLoadItems(ProtoHttpClient http, Popup popup, int pageSize = 200)
    {
        int page = 1;
        int loaded = 0;

        ItemDict.Clear();
        ItemCategoryDict.Clear();

        while (true)
        {
            string url = $"{ApiRoutes.Items}?page={page}&pageSize={pageSize}&activeOnly=true";
            yield return http.Get(url, ListItemsResponse.Parser, (ApiResult<ListItemsResponse> res) =>
            {
                if (!res.Ok)
                {
                    popup?.Show($"아이템 불러오기 실패: {res.Message}");
                    return;
                }
                _items = res.Data;
            });

            if (_items == null || _items.Items.Count == 0)
                break;

            foreach (var item in _items.Items)
            {
                ItemDict[item.Id] = item;
                ItemCategoryDict[item.Id] = item.TypeId;
            }

            loaded += _items.Items.Count;
            Debug.Log($"[ItemCache] {loaded}/{_items.TotalCount} 불러옴");

            if (loaded >= _items.TotalCount)
                break;

            page++;
        }

        Debug.Log($"[ItemCache] 최종 {ItemDict.Count}개 로드 완료");
    }
    public IEnumerator CoGetItemDetail(ProtoHttpClient http, Popup popup, long itemId, System.Action<ItemMessage> onLoaded)
    {
        string url = $"{ApiRoutes.Items}/{itemId}";
        GetItemResponse? response = null;

        yield return http.Get(url, GetItemResponse.Parser, (ApiResult<GetItemResponse> res) =>
        {
            if (!res.Ok)
            {
                popup?.Show($"아이템 상세 불러오기 실패: {res.Message}");
                return;
            }
            response = res.Data;
        });

        if (response != null)
        {
            onLoaded?.Invoke(response.Item);
            Debug.Log($"[ItemCache] 아이템 상세 로드 완료: {response.Item.Name}");
        }
    }
    public ItemMessage? GetSummary(long id)
    {
        return ItemDict.TryGetValue(id, out var item) ? item : null;
    }
}
