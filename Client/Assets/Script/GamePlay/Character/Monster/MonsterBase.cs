using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebServer.Protos.Monsters;

public class MonsterBase : MonoBehaviour
{
    [HideInInspector] public MonsterPb MonsterData;
    [SerializeField] private MonsterAppearance Appearance; 
    [SerializeField] private MonsterAnimationController Controller;

    public void Set(MonsterPb enemyPb)
    {
        MonsterData = enemyPb;
        Appearance.Set(MonsterData.Id);

    }
}
