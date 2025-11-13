using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebServer.Protos.Monsters;

public class MonsterBase : CombatActorView
{
    [HideInInspector] public MonsterPb MonsterData;
    [SerializeField] private MonsterAppearance Appearance; 
    [SerializeField] private MonsterAnimationController Controller;

    public void Set(MonsterPb enemyPb)
    {
        MonsterData = enemyPb;
        Appearance.Set(MonsterData.Id);

    }
    public override void PlayHitFx(bool isCrit)
    {
        base.PlayHitFx(isCrit); 
    }

    protected override void OnDie()
    {
        base.OnDie(); 
    }
}
