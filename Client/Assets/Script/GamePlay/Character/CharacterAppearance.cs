using Contracts.CharacterModel;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class CharacterAppearance : MonoBehaviour
{
    [SerializeField] private CharacterAnimationController animController;

    [Header("아이템 모델")]
    [SerializeField] private MeshFilter WeaponR_Mesh;
    [SerializeField] private MeshFilter WeaponL_Mesh;

    [SerializeField] private MeshFilter Head_Mesh;
    [SerializeField] private MeshFilter Hair_Mesh;
    [SerializeField] private MeshFilter Mouth_Mesh;
    [SerializeField] private MeshFilter Eye_Mesh;
    [SerializeField] private MeshFilter Acc_Mesh;

    public void Set(CharacterModelPb modelData)
    {
        Set_Size(modelData.BodySize);
        Set_Animator(modelData.Animation.ToString());
    }

    private void Set_Size(BodySizePb size)
    {
        if (size == BodySizePb.Normal)
        {
            this.gameObject.transform.localScale = Vector3.one;
        }
        else if (size == BodySizePb.Small)
        {
            this.gameObject.transform.localScale = Vector3.one * 0.75f;
        }
        else
        {
            this.gameObject.transform.localScale = Vector3.one * 1.25f;
        }

    }
    private async void Set_Animator(string key)
    {
        var controller = await AddressableManager.Instance.LoadAsync<RuntimeAnimatorController>(key);
         
        if (controller != null)
        {
            animController.Set_Controller(controller); 
        }
        else
        {
            Debug.LogError($"Animator Controller '{key}' 로드 실패");
        }
    }
    private async void Set_Model(MeshFilter filter, int? modelId)
    {
        // [1] 모델 ID를 바탕으로 실제 Key를 찾아야함
        filter.mesh = modelId == null? null : null;

    }
}
