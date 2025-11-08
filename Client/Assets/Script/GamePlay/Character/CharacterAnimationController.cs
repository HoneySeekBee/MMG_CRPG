using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Set_Controller(RuntimeAnimatorController controller)
    {
        animator.runtimeAnimatorController = controller;
    }
    public void PlayIdle(bool battle)
    {
        animator.SetBool("InBattle", battle);
    }

    public void PlayMove(float speed)
    {
        animator.SetBool("IsMoving", speed > 0.01f);
        animator.SetFloat("MoveSpeed", speed);
    }

    public void PlayAttack(int type)
    {
        // 1 = normal, 2 = crit
        animator.SetTrigger(type == 1 ? "Attack01" : "Attack02");
    }

    public void PlaySkill()
    {
        animator.SetTrigger("Skill");
    }

    public void PlayDie()
    {
        animator.SetTrigger("Die");
    }
}
