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
        if (CanPlayAnim(CombatActorView.ActionState.Damage))
            Animator.Play_GetHit(isCrit);
    }

    public override void OnDie()
    {
        if (CanPlayAnim(CombatActorView.ActionState.Dead))
            Animator.PlayDie();
        base.OnDie();
    }
    protected override void UpdateHPBar()
    {
        HpUI.Set((float)Hp / MaxHp);
    }

    public override void PlayMove()
    {
        if (CanPlayAnim(CombatActorView.ActionState.Move))
            Animator.PlayMove(1);
    }
    public override void PlayIdle()
    {
        if (CanPlayAnim(CombatActorView.ActionState.Idle))
            Animator.PlayIdle(true);
    }
    public override void PlayAttack(bool isCrit)
    {
        State = ActionState.Attack;
        Animator.PlayAttack(isCrit);
    }

    public override void ApplySpeed(float scale)
    {
        Animator.Set_CombatSpeed(scale);
    }
}
