using Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CombatTeam
{
    Player = 0,
    Enemy = 1
}
public abstract class CombatActorView : MonoBehaviour, ICombatSpeedAffectable
{  
    [Header("Runtime Info")]
    public long ActorId;        // 서버 ActorId
    public CombatTeam Team;

    public int MaxHp;
    public int Hp;

    public float SpawnX { get; private set; }
    public float SpawnZ { get; private set; }

    [Header("Optional")]
    public GameObject HitEffect;
    public GameObject DeadEffect;

    [Header("Look At")]
    [SerializeField] private Transform visualRoot;      
    [SerializeField] private float turnSpeed = 18f;     
    [SerializeField] private float minMoveSqr = 0.0004f;  
    [SerializeField] private float yawOffset = 0f;

    private Vector3 _lastPos;
    private bool _facingInit;

    // 최근 상태 
    public enum ActionState { None, Idle, Move, Attack, Dead, Damage, Victory }
    public ActionState State = ActionState.None;
    protected virtual void Awake()
    {
        if (visualRoot == null) visualRoot = transform;
    }
    public void UpdateFacingByMovement(float dt)
    {
        var cur = transform.position;

        if (!_facingInit)
        {
            _lastPos = cur;
            _facingInit = true;
            return;
        }

        Vector3 delta = cur - _lastPos;
        _lastPos = cur;

        delta.y = 0f;

        if (delta.sqrMagnitude < minMoveSqr) return;

        var dir = delta.normalized;
        var targetRot = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(0f, yawOffset, 0f);

        visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRot, turnSpeed * dt);
    }
    public void FaceDirection(Vector3 dir, bool smooth)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        var targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

        // 모델 축 보정 필요하면 yawOffset 적용
        targetRot *= Quaternion.Euler(0f, yawOffset, 0f);

        if (visualRoot == null) visualRoot = transform;

        if (!smooth)
        {
            visualRoot.rotation = targetRot;
        }
        else
        {
            // 부드럽게 돌리려면 코루틴/트윈으로 처리
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRot, 1f);
        }

        ResetFacingCache(); // 이동방향 기반 캐시 꼬임 방지
    }
    public void SetSpawnPosition(Vector3 pos)
    {
        SpawnX = pos.x;
        SpawnZ = pos.z;
        State = ActionState.None;
    }

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
    public virtual void PlayVictory()
    {

    }
    public abstract void ApplySpeed(float scale);
    
    protected bool CanPlayAnim(ActionState newAction)
    {
        if (State == newAction)
            return false;

        State = newAction;
        return true;
    }
    public void ResetFacingCache()
    {
        _facingInit = false;
    }
}
