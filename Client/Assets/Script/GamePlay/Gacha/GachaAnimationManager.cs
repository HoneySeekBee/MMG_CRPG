using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaAnimationManager : MonoBehaviour
{
    public static GachaAnimationManager Instance { get; private set; }

    [SerializeField] private GameObject ParentObj;
    [SerializeField] private CinemachineDollyCart dollyCart;
    [SerializeField] private CinemachineSmoothPath dollyPath;
    [SerializeField] private float moveDuration = 1.5f;

    [SerializeField] private Animator GateAnimator;

    [System.Serializable]
    public class FlagGroup
    {
        public Animator left;
        public Animator right;
        public bool triggered;
    }

    [SerializeField] private FlagGroup[] flags;

    private int waypointCount;
    private bool isSkip = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void Skip()
    {
        isSkip = true;
    }

    private float PlaybackSpeed => isSkip ? 2f : 1f;

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time / PlaybackSpeed);
    }

    // =========== 가챠 실행 ===========
    public void PlayGacha()
    {
        StartCoroutine(_Play());
    }

    private IEnumerator _Play()
    {
        ParentObj.SetActive(true);
        yield return null;

        Init();
        StartCoroutine(Show_Animation());
    }

    public void DisableGacha()
    {
        ParentObj.SetActive(false);
    }

    private void Init()
    {
        StopAllCoroutines();
        isSkip = false;

        waypointCount = dollyPath.m_Waypoints.Length;

        dollyCart.m_PositionUnits = CinemachinePathBase.PositionUnits.Normalized;
        dollyCart.m_Position = 0f;

        GateAnimator.Play("Close_Idle", 0, 0);
        GateAnimator.Update(0);

        foreach (var f in flags)
        {
            if (f.left != null)
            {
                f.left.speed = 1;
                PlayAnimationAtTime(f.left, "idle_A", 0f);
            }

            if (f.right != null)
            {
                f.right.speed = 1;
                PlayAnimationAtTime(f.right, "idle_A", 0f);
            }
        }

        PrepareFlagsDeployIdle();
    }

    private void PlayAnimationAtTime(Animator anim, string stateName, float startTime)
    {
        float clipLength = GetClipLength(anim, stateName);
        float normalized = (clipLength > 0) ? startTime / clipLength : 0f;

        anim.Play(stateName, 0, normalized);
        anim.Update(0f);
    }

    private float GetClipLength(Animator anim, string clipName)
    {
        foreach (var c in anim.runtimeAnimatorController.animationClips)
        {
            if (c.name == clipName) return c.length;
        }
        return 0.1f;
    }

    // =========== 연출 ===========

    private IEnumerator Show_Animation()
    {
        PrepareFlagsDeployIdle();

        // 이동 0 → 1
        StartCoroutine(MoveToWaypoint(1, 1f));
        yield return Wait(0.5f);

        // Gate 열기, Animator 속도 적용
        GateAnimator.speed = PlaybackSpeed;
        GateAnimator.Play("Open");
        yield return Wait(0.5f);

        // 이동 1 → 2
        StartCoroutine(MoveToWaypoint(2, 3.5f));

        // 깃발 deploy
        for (int i = 0; i < flags.Length; i++)
        {
            if (flags[i].left != null)
            {
                PlayAnimationAtTime(flags[i].left, "deploy", 0.08f);
                flags[i].left.speed = PlaybackSpeed;
            }

            if (flags[i].right != null)
            {
                PlayAnimationAtTime(flags[i].right, "deploy", 0.08f);
                flags[i].right.speed = PlaybackSpeed;
            }

            yield return Wait(0.5f);
        }

        Debug.Log("가챠 연출 끝!");
    }

    private IEnumerator MoveToWaypoint(int index, float duration)
    {
        float startPos = dollyCart.m_Position;
        float targetPos = GetNormalizedPos(index, waypointCount);

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime * PlaybackSpeed;

            float t = Mathf.SmoothStep(0, 1, time / duration);
            dollyCart.m_Position = Mathf.Lerp(startPos, targetPos, t);

            yield return null;
        }

        dollyCart.m_Position = targetPos;
    }

    private float GetNormalizedPos(int index, int total)
    {
        return (float)index / (total - 1);
    }

    private void PrepareFlagsDeployIdle()
    {
        foreach (var f in flags)
        {
            if (f.left != null) FreezeAnimationAtStart(f.left, "deploy");
            if (f.right != null) FreezeAnimationAtStart(f.right, "deploy");
        }
    }

    private void FreezeAnimationAtStart(Animator anim, string state)
    {
        anim.Play(state, 0, 0);
        anim.Update(0);
        anim.speed = 0;
    }
}
