using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashCameraBehaviour : BaseCameraBehaviour
{
    
    
    
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // setup
        StartState(animator);
    }
    
    
    
    

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        Update_TimeoutTimer();
        Update_Lock();
        if (!Update_EnvironmentCrashDetectSensorActions())
        {
            Update_CrashDetectBack();
        }
    }
    
}
