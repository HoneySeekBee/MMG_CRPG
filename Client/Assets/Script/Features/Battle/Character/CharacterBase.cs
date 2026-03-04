using Contracts.CharacterModel;
using Game.Logging;
using System;
using System.Collections;
using System.Threading.Tasks;
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
        // �÷��̾� ĳ���͸��� �ǰ� ����
        if (CanPlayAnim(CombatActorView.ActionState.Damage))
            Animator.Play_GetHit(isCrit);
    }

    public override void OnDie()
    {
        StartCoroutine(PlayDie());
    }
    private IEnumerator PlayDie()
    { 
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

    public async Task Set(CharacterModelPb modelData, bool isBattle = false)
    {
        Appearance.Set(modelData, isBattle);
        await Set_Animator(modelData.Animation.ToString());
        if (CanPlayAnim(CombatActorView.ActionState.Idle))
            Animator.PlayIdle(false);
    }
    private async Task Set_Animator(string key)
    {
        try
        {
            var controller = await AddressableManager.Instance.LoadAsync<RuntimeAnimatorController>(key + "_CONTROLLER");
            Animator.Set_Controller(controller);
        }
        catch (Exception e)
        {
            GameLogger.Error($"[CharacterBase] Set_Animator failed: {e.Message}");
        }
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
        StartCoroutine(CoResetAttackState());
    }
    public override void PlayVictory()
    {
        if (CanPlayAnim(CombatActorView.ActionState.Victory))
            Animator.PlayVictory();
    }
}
