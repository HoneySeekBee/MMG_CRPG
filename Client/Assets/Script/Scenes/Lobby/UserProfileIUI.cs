using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Contracts.Protos;
using Unity.VisualScripting.FullSerializer;

namespace Game.Lobby
{
    public class UserProfileIUI : MonoBehaviour
    {
        public ApiConfig Config;
        public TMP_Text LevelText;
        public TMP_Text NickNameText;
        public Image IconImage;

        public Animator ExpAnimator;
        [Tooltip("EXP 바가 들어있는 기본 상태(클립) 이름")]
        public string ExpClipStateName = "ExpFill";
        public TMP_Text ExpText;
        public void Set(UserProfilePb p, int nextLevelExp = 100)
        {
            if (LevelText) LevelText.text = $"Lv {p.Level}";
            if (NickNameText) NickNameText.text = p.Nickname;

            // EXP 퍼센트 계산 (0~1)
            float percent = 0f;
            if (nextLevelExp > 0) percent = Mathf.Clamp01((float)p.Exp / nextLevelExp);

            if (ExpText) ExpText.text = $"EXP {p.Exp:N0} / {nextLevelExp:N0} ({percent * 100f:0.#}%)";
            SetExpFill(percent);

            if (IconImage && !string.IsNullOrEmpty(p.IconKey) && Config != null)
                StartCoroutine(RemoteIconLoader.LoadInto(
                target: IconImage,
                    baseUrl: Config.BaseUrl,  
                    iconKey: p.IconKey,
                    version: p.IconVersion
                ));
        }
        public void SetExpFill(float normalized)
        {
            if (!ExpAnimator) return;

            normalized = Mathf.Clamp01(normalized);

            // 애니메이터를 '해당 포즈'에 고정
            // 1) 재생 속도 0으로 멈춤
            ExpAnimator.speed = 0f;

            // 2) 원하는 정규화 시간으로 이동
            //    (stateName, layer = 0, normalizedTime = 0~1)
            ExpAnimator.Play(ExpClipStateName, 0, normalized);

            // 3) 즉시 적용(1프레임 기다리지 않도록)
            ExpAnimator.Update(0f);
        }
        public void TweenExpFill(float from, float to, float duration)
        {
            if (!gameObject.activeInHierarchy) { SetExpFill(to); return; }
            StartCoroutine(CoTweenExp(from, to, duration));
        }

        private System.Collections.IEnumerator CoTweenExp(float from, float to, float duration)
        {
            from = Mathf.Clamp01(from);
            to = Mathf.Clamp01(to);
            duration = Mathf.Max(0.01f, duration);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                SetExpFill(Mathf.Lerp(from, to, k));
                yield return null;
            }
            SetExpFill(to);
        }

        // 실제 프로젝트에 맞게 구현 (Addressables, SpriteAtlas 등)
        private void LoadIcon(int iconId, Image target)
        {
            if (iconId <= 0 || target == null) { target.enabled = false; return; }

            var sprite = Resources.Load<Sprite>($"Icons/icon_{iconId}");
            target.sprite = sprite;
            target.enabled = sprite != null;
            if (sprite != null) target.preserveAspect = true;
        }
    }

}