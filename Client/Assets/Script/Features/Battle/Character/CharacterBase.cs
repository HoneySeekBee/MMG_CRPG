using Contracts.CharacterModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : CombatActorView
{
    [SerializeField] protected HpCanvasController HpUI;
    public CharacterAnimationController Animator;
    [SerializeField] private CharacterAppearance Appearance;

    #region About Combat
    public override void PlayHitFx(bool isCrit)
    {
        base.PlayHitFx(isCrit);
        // 플레이어 캐릭터만의 피격 연출
        if (CanPlayAnim(CombatActorView.ActionState.Damage))
            Animator.Play_GetHit(isCrit);
    }

    public override void OnDie()
    {
        StartCoroutine(PlayDie());
    }
    private IEnumerator PlayDie()
    {
        // 파티원 사망 UI, 부활 가능 등
        if (CanPlayAnim(CombatActorView.ActionState.Dead))
            Animator.PlayDie();
        yield return new WaitForSeconds(1);
        base.OnDie();
    }
    protected override void UpdateHPBar()
    {
        HpUI.Set((float)Hp / MaxHp);
    }
    #endregion

    public void Set(CharacterModelPb modelData, bool isBattle = false)
    {
        Appearance.Set(modelData, isBattle);
        Set_Animator(modelData.Animation.ToString());
        if (CanPlayAnim(CombatActorView.ActionState.Idle))
            Animator.PlayIdle(false);
    }
    private async void Set_Animator(string key)
    {
        var controller = await AddressableManager.Instance.LoadAsync<RuntimeAnimatorController>(key + "_CONTROLLER");
        Animator.Set_Controller(controller);
    }

    public override void ApplySpeed(float scale)
    {
        Animator.Set_CombatSpeed(scale);
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
    public override void PlayVictory()
    {
        if (CanPlayAnim(CombatActorView.ActionState.Victory))
            Animator.PlayVictory();
    }
}
