using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpCanvasController : MonoBehaviour
{
    [SerializeField] private Image innerImage; 
    void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }

    public void Set(float percent)
    {
        innerImage.DOKill();
        if (percent >= 1f)
        {
            innerImage.fillAmount = 1f;
            return;
        }
        innerImage
        .DOFillAmount(percent, 0.25f)  // 0.25s 동안 부드럽게
        .SetEase(Ease.OutQuad);
    }
}
