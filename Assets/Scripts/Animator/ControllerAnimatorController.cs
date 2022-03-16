using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerAnimatorController : AnimatorController
{
    [SerializeField] private Animator animator;
    public void ToIdle()
    {
        animator.SetBool("GoForward", false);
        animator.SetBool("GoBackward", false);
        animator.SetBool("GoLeft", false);
        animator.SetBool("GoRight", false);
    }
    public void WalkForward()
    {
        animator.SetBool("GoForward", true);
        animator.SetBool("GoBackward", false);
        animator.SetBool("GoLeft", false);
        animator.SetBool("GoRight", false);
    }
    public void WalkBackward()
    {
        animator.SetBool("GoForward", false);
        animator.SetBool("GoBackward", true);
        animator.SetBool("GoLeft", false);
        animator.SetBool("GoRight", false);
    }
    public void WalkLeft()
    {
        animator.SetBool("GoForward", false);
        animator.SetBool("GoBackward", false);
        animator.SetBool("GoLeft", true);
        animator.SetBool("GoRight", false);
    }
    public void WalkRight()
    {
        animator.SetBool("GoForward", false);
        animator.SetBool("GoBackward", false);
        animator.SetBool("GoLeft", false);
        animator.SetBool("GoRight", true);
    }
    public void PlayJump()
    {
        animator.SetTrigger("Jump");
    }
    public void PlayDash()
    {
        animator.SetTrigger("Dash");
    }
    public void PlayLanding()
    {
        animator.SetTrigger("JumpLanding");
    }

    public void SetInAir(bool v)
    {
        animator.SetBool("InAir", v);
    }
    public void SetOnWall(bool v)
    {
        animator.SetBool("OnWall", v);
    }
    public void SetLeftWall(bool v)
    {
        animator.SetBool("LeftWall", v);
    }
    public void SetSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
        Debug.Log("ControllerAnimator, SetSpeed : clamped speed = " + speed);
    }
}
