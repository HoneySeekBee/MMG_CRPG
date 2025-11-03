using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Lobby
{
    public class UIPopupPool : UIPrefabPool
    {
        public async Task<T> ShowPopupAsync<T>(string key, Transform parent, object data = null)
        where T : UIPopup
        {
            var go = await ShowAsync(key, parent); // base의 ShowAsync (인스턴스 만들거나 재사용)
            if (!go) return null;

            var popup = go.GetComponent<T>();
            if (!popup)
            {
                Debug.LogError($"[{nameof(UIPopupPool)}] {key} 프리팹에 {typeof(T).Name}이 없음");
                return null;
            }

            popup.Initialize();
            if (data != null) popup.SetData(data);
            await popup.ShowAsync();
            return popup;
        }
        public async Task HidePopupAsync(string key, UIPopup popup)
        {
            if (!popup) return;
            await popup.HideAsync();          // 애니메이션 닫기
            Hide(key, popup.gameObject);      // 풀에 반납
        }

    }
}