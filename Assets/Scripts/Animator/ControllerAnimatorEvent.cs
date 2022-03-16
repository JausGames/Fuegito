using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerAnimatorEvent : AnimatorEvent
{
    [SerializeField] PlayerController controller;
    
    public void FallEvent()
    {
        //controller.StartFalling();        
    }
    public void JumpControllerEvent()
    {
        //controller.Jump();
    }
    public void MoveTrueEvent()
    {
        controller.SetCanMove(true);
    }
    public void MoveFalseEvent()
    {
        controller.SetCanMove(false);
    }
}
