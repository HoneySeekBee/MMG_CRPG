using Contracts.CharacterModel;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

    private Vector3 PartySetSize = Vector3.one;
    private Vector3 BattleSize = Vector3.one * 0.5f;
    public void Set(CharacterModelPb modelData, bool isBattle = false)
    {
        Set_Size(modelData.BodySize, isBattle ? BattleSize : PartySetSize);
        Set_Animator(modelData.Animation.ToString());
        CharacterCache characterCache = CharacterCache.Instance;

        string weaponLCode = modelData.WeaponLId == null ? string.Empty : characterCache.WeaponPartsById[modelData.WeaponLId ?? 0].Code;
        string weaponRCode = modelData.WeaponRId == null ? string.Empty : characterCache.WeaponPartsById[modelData.WeaponRId?? 0].Code;
        string headCode = modelData.PartHeadId == null ? string.Empty : characterCache.ModelPartsById[modelData.PartHeadId?? 0].PartKey;
        string hairCode = modelData.PartHairId == null ? string.Empty : characterCache.ModelPartsById[modelData.PartHairId?? 0].PartKey;
        string mouthCode = modelData.PartMouthId == null ? string.Empty : characterCache.ModelPartsById[modelData.PartMouthId?? 0].PartKey;
        string eyeCode = modelData.PartEyeId == null ? string.Empty : characterCache.ModelPartsById[modelData.PartEyeId?? 0].PartKey;
        string accCode = modelData.PartAccId == null ? string.Empty : characterCache.ModelPartsById[modelData.PartAccId ?? 0].PartKey;
        Set_Model(WeaponL_Mesh, weaponLCode, true);
        Set_Model(WeaponR_Mesh, weaponRCode, true);
        Set_Model(Head_Mesh, headCode);
        Set_Model(Hair_Mesh, hairCode);
        Set_Model(Mouth_Mesh, mouthCode);
        Set_Model(Eye_Mesh, eyeCode);
        Set_Model(Acc_Mesh, accCode);
        SetMeshColor(Hair_Mesh.GetComponent<MeshRenderer>(), modelData.HairColorCode);
        SetMeshColor(Head_Mesh.GetComponent<MeshRenderer>(), modelData.SkinColorCode);
    }

    private void Set_Size(BodySizePb size, Vector3 standardSize)
    {
        if (size == BodySizePb.Normal)
        {
            SetWorldScale(this.gameObject.transform, standardSize);
        }
        else if (size == BodySizePb.Small)
        {
            SetWorldScale(this.gameObject.transform, standardSize * 0.75f); 
        }
        else
        {
            SetWorldScale(this.gameObject.transform, standardSize * 1.25f); 
        }

    }
    void SetWorldScale(Transform t, Vector3 worldScale)
    {
        var parent = t.parent;
        if (parent == null)
        {
            t.localScale = worldScale;
        }
        else
        {
            // 부모의 실제 스케일을 나눠서 상대 스케일로 환산
            var parentScale = parent.lossyScale;
            t.localScale = new Vector3(
                worldScale.x / parentScale.x,
                worldScale.y / parentScale.y,
                worldScale.z / parentScale.z
            );
        }
    }
    private async void Set_Animator(string key)
    {
        var controller = await AddressableManager.Instance.LoadAsync<RuntimeAnimatorController>(key + "_CONTROLLER");
         
        if (controller != null)
        {
            animController.Set_Controller(controller); 
        }
        else
        {
            Debug.LogError($"Animator Controller '{key + "_CONTROLLER"}' 로드 실패");
        }
    }
    public void Set_Model(MeshFilter filter, string modelKey, bool isWeapon = false)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            filter.sharedMesh = null;
            return;
        }

        var mesh = isWeapon == false? CharacterCache.Instance.GetCharacterMesh(modelKey) : CharacterCache.Instance.GetWeaponMesh(modelKey);
        if (mesh == null)
        {
            Debug.LogWarning($"mesh not found: {modelKey} {isWeapon}");
            return;
        }

        filter.sharedMesh = mesh; 
    }
    public void SetMeshColor(MeshRenderer meshRender, string hexColor)
    {
        if (meshRender == null)
        {
            Debug.LogWarning("meshRender가 없습니다!");
            return;
        }
         
        if (meshRender == null)
        {
            Debug.LogWarning("MeshRenderer를 찾을 수 없습니다!");
            return;
        }
        string colorCode = "#" + hexColor;
        if (UnityEngine.ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            // 머티리얼 인스턴스 생성 (공유 머티리얼을 직접 바꾸면 다른 캐릭터도 바뀔 수 있음)
            var mat = meshRender.material;
            mat.color = color;

            Debug.Log($"머리 색상 변경 완료: {hexColor} → {color}");
        }
        else
        {
            Debug.LogWarning($"잘못된 색상 코드: {hexColor}");
        }
    }
}
