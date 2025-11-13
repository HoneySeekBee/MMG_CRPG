using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBase : MonoBehaviour
{
    [HideInInspector] public EnemyPb MonsterData;
    [SerializeField] private MonsterAppearance Appearance; 
    [SerializeField] private MonsterAnimationController Controller;

    public void Set(EnemyPb enemyPb)
    {
        MonsterData = enemyPb;
        Appearance.Set(MonsterData.EnemyCharacterId);

    }
}
