using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed class CombatSpeedApplier : MonoBehaviour
{
    [SerializeField] bool includeInactive = true;

    readonly List<ICombatSpeedAffectable> _targets = new();

    void Awake()
    {
        CacheTargets();
    }
    public void RefreshAndApply(float scale)
    {
        CacheTargets();
        ApplySpeed(scale);
    }
    public void CacheTargets()
    {
        _targets.Clear();

        var scene = gameObject.scene; 
        var roots = scene.GetRootGameObjects();

        foreach (var root in roots)
        {
            var monos = root.GetComponentsInChildren<MonoBehaviour>(includeInactive);
            foreach (var m in monos)
            {
                if (m is ICombatSpeedAffectable t)
                    _targets.Add(t);
            }
        }
    }

    public void ApplySpeed(float scale)
    {
        for (int i = 0; i < _targets.Count; i++)
            _targets[i].ApplySpeed(scale);
    }
}