using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashCameraBehaviour : BaseCameraBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // setup
        _driverCinematicCamBehaviourDictionary = animator.GetComponent<DriverCinematicCamBehaviourDictionary>();
        _driverCrashDetector = animator.GetComponentInParent<DriverCrashDetector>();
        _driverCinematicCam = animator.GetComponent<DriverCinematicCam>();
        animator = this.animator;
    }
    
    
    
    

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // do environmental crash trigger
        EnvironmentCrashDetectSensorActions();
        // if we have not 
    }
    
}
