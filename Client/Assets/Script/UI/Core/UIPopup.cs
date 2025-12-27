using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Lobby
{
    public class UIPopup : MonoBehaviour
    {
        [Header("Common")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected bool blockRaycastWhenShown = true;
        [SerializeField] protected float fadeDuration = 0.15f;
         
        private bool _initialized;

        // [1] 초기화 코드
        public virtual void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            if (!canvasGroup) canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        // [2] 데이터 주입 (필요시) 
        public virtual void SetData<T>(T data) { /* 파생 클래스에서 사용 */ }

        // [3] 보여주기 
        public async virtual Task ShowAsync()
        {
            gameObject.SetActive(true);
            OnBeforeShow();

            await FadeAsync(1f, fadeDuration);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = blockRaycastWhenShown;

            OnAfterShow();
        }

        // [4] 비활성화 
        public async virtual Task HideAsync()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            OnBeforeHide();
            await FadeAsync(0f, fadeDuration);
            gameObject.SetActive(false);
            OnAfterHide();
        }

        protected virtual void OnBeforeShow() { }
        protected virtual void OnAfterShow() { }
        protected virtual void OnBeforeHide() { }
        protected virtual void OnAfterHide() { }

        protected async Task FadeAsync(float target, float duration)
        {
            if (duration <= 0f) { canvasGroup.alpha = target; return; }

            float start = canvasGroup.alpha;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                canvasGroup.alpha = Mathf.Lerp(start, target, t);
                await Task.Yield();
            }
            canvasGroup.alpha = target;
        }
    }

}