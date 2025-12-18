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
    public void Set_CombatSpeed(float scale)
    {
        animator.speed = scale;
    }
    public void Play_GetHit(bool isCrit)
    { 
        string key = isCrit == false ? "GetHit01" : "GetHit02";
        animator.Play(key);
    }
    public void PlayIdle(bool battle)
    { 
        animator.Play("Idle_Battle", 0, 0);
    }
    public void PlayVictory()
    { 
        animator.Play("Victory", 0, 0);
    }
    public void PlayMove(float speed)
    { 
        animator.Play("MoveFWD", 0, 0); 
    }

    public void PlayAttack(bool isCrit)
    { 
        string key = isCrit == false ? "Attack01" : "Attack02";
        animator.Play(key, 0, 0);
    }

    public void PlaySkill()
    { 
        animator.Play("Skill", 0, 0);
    }

    public void PlayDie()
    { 
        animator.Play("Die", 0, 0);
    }
}
