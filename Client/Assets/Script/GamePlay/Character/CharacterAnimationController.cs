using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    bool isMove;
    public void Set_Controller(RuntimeAnimatorController controller)
    {
        isMove = false;
        animator.runtimeAnimatorController = controller;
    }
    public void Play_GetHit(bool isCrit)
    {
        isMove = false;
        string key = isCrit == false ? "GetHit01" : "GetHit02";
        animator.Play(key);
    }
    public void PlayIdle(bool battle)
    {
        isMove = false;
        animator.Play("Idle_Battle", 0, 0);
    }

    public void PlayMove(float speed)
    {
        if (isMove)
            return;
        isMove = true;
        animator.SetTrigger("isMove");
    }

    public void PlayAttack(bool isCrit)
    {
        isMove = false;
        string key = isCrit == false ? "Attack01" : "Attack02";
        animator.Play(key, 0, 0);
    }

    public void PlaySkill()
    {
        isMove = false;
        animator.Play("Skill", 0, 0);
    }

    public void PlayDie()
    {
        isMove = false;
        animator.Play("Die", 0, 0);
    }
}
