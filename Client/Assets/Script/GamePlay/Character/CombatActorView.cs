using Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CombatTeam
{
    Player = 0,
    Enemy = 1
}
public class CombatActorView : MonoBehaviour
{
    [Header("Runtime Info")]
    public long ActorId;        // 서버 ActorId
    public CombatTeam Team;

    public int MaxHp;
    public int Hp;

    [Header("Optional")]
    public GameObject HitEffect;
    public GameObject DeadEffect;
     
    public virtual void InitFromServer(long actorId, int team, int hp)
    {
        ActorId = actorId;
        Team = (CombatTeam)team;
        MaxHp = hp;
        Hp = hp;
        UpdateHPBar();
    }
    public virtual void ApplyDamage(int damage, bool isCrit)
    {
        Hp = Mathf.Max(0, Hp - damage);
        UpdateHPBar();
        PlayHitFx(isCrit);

        if (Hp <= 0)
        {
            OnDie();
        }
    }

    // HP 세팅용 (서버에서 full sync 할 일 있을 때)
    public virtual void SetHp(int hp)
    {
        Hp = Mathf.Clamp(hp, 0, MaxHp);
        UpdateHPBar();
    }

    protected virtual void UpdateHPBar()
    {
        // TODO: HP바 UI 업데이트 (체력바 있으면 여기서)
    }

    public virtual void PlayHitFx(bool isCrit)
    {
        if (HitEffect != null)
            Instantiate(HitEffect, transform.position, Quaternion.identity);

        // TODO: 피격 애니메이션, 크리일 때 살짝 다르게, 카메라 흔들기 등
    }

    public virtual void OnDie()
    {
        if (DeadEffect != null)
            Instantiate(DeadEffect, transform.position, Quaternion.identity);

        // 기본 구현: 그냥 꺼버리기
        gameObject.SetActive(false);
    }

    public virtual void PlayMove()
    {

    }

    public virtual void PlayIdle()
    {

    }
    public virtual void PlayAttack(bool isCrit)
    {

    }

}
