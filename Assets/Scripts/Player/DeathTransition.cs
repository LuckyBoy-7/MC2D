using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTransition : Singleton<DeathTransition>
{
    private Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    public void TryPlay()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Transition"))
            animator.Play("Transition");
    }

    public void OnAnimationExecuteHalf()
    {
        PlayerFSM.instance.OnTransitionOver();
    }
}