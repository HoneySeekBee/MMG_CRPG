using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebServer.Protos.Monsters;

public class MonsterBase : CombatActorView
{
    [HideInInspector] public MonsterPb MonsterData;
    [SerializeField] private MonsterAppearance Appearance;
    [SerializeField] private MonsterAnimationController Animator;
    [SerializeField] protected HpCanvasController HpUI;
    public void Set(MonsterPb enemyPb)
    {
        MonsterData = enemyPb;
        Appearance.Set(MonsterData.Id, Animator.Set); 
    }
    public override void PlayHitFx(bool isCrit)
    {
        base.PlayHitFx(isCrit);
        Animator.Play_GetHit(isCrit);
    }

    public override void OnDie()
    {
        Animator.PlayDie();
        base.OnDie();
    }
    protected override void UpdateHPBar()
    {
        HpUI.Set((float)Hp / MaxHp);
    }

    public override void PlayMove()
    {
        Animator.PlayMove(1);
    }
    public override void PlayIdle()
    {
        Animator.PlayIdle(true);
    }
    public override void PlayAttack(bool isCrit)
    {
        Animator.PlayAttack(isCrit);
    }
}
