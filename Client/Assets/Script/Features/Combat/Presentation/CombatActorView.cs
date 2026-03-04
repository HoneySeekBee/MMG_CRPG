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
    public long ActorId;      
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

        targetRot *= Quaternion.Euler(0f, yawOffset, 0f);

        if (visualRoot == null) visualRoot = transform;

        if (!smooth)
        {
            visualRoot.rotation = targetRot;
        }
        else
        {
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRot, 1f);
        }

        ResetFacingCache(); 
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
     
    public virtual void SetHp(int hp)
    {
        Hp = Mathf.Clamp(hp, 0, MaxHp);
        UpdateHPBar();
    }

    protected virtual void UpdateHPBar()
    {
        // TODO: HP UI 연결
    }

    public virtual void PlayHitFx(bool isCrit)
    {
        if (HitEffect != null)
            Instantiate(HitEffect, transform.position, Quaternion.identity);
         
    }

    public virtual void OnDie()
    {
        if (DeadEffect != null)
            Instantiate(DeadEffect, transform.position, Quaternion.identity);
         
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
    /// <summary>
    /// 웨이브 복귀 전용 - yawOffset만 적용한 기본 방향으로 리셋 (FaceDirection의 이중 적용 방지)
    /// </summary>
    public void FaceDefaultDirection()
    {
        if (visualRoot == null) visualRoot = transform;
        visualRoot.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up) * Quaternion.Euler(0f, yawOffset, 0f);
        ResetFacingCache();
    }
    public void ResetFacingCache()
    {
        _facingInit = false;
    }
}
