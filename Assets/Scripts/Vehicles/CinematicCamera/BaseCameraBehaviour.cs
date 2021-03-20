using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class BaseCameraBehaviour : StateMachineBehaviour
{
    public float minimumCameraDuration = 1f;
    public bool setCam = true;
    public DriverCinematicCam.Cams stateCam;
    
    protected DriverCrashDetector _driverCrashDetector;
    protected DriverCinematicCam _driverCinematicCam;
    protected DriverCinematicCamBehaviourDictionary _driverCinematicCamBehaviourDictionary;
    protected DriverCrashDetector.CurrentSensorReportStruct currentSensorReport;
    protected Animator myAnimator;
    public float crashThreshold = 1f;
    
    // crash into environment 
    public string crashTriggerEnvironmentDetectAhead = "environmentCrashAhead";
    public string crashTriggerEnvironmentDetectLeft = "environmentCrashLeft";
    public string crashTriggerEnvironmentDetectRight = "environmentCrashRight";
    // when we are gonna crash into someone
    public string crashTriggerPlayerDetectAhead = "playerCrashAhead";
    public string crashTriggerPlayerDetectLeft = "playerCrashLeft";
    public string crashTriggerPlayerDetectRight = "playerCrashRight";
    // when someone is gonna crash into us
    // ignore for now
    public string crashTriggerIncomingPlayerDetect = "playerIncomingCrash";

    // used for when we need to return state
    public string backTrigger = "back";


    protected bool crashTimeout = false;

    protected float cooldown = 2f;
    
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StartState(animator);
    }

    protected void StartState(Animator animatorLocal)
    {
        myAnimator = animatorLocal;
        // setup
        _driverCinematicCamBehaviourDictionary = myAnimator.GetComponent<DriverCinematicCamBehaviourDictionary>();
        _driverCrashDetector = myAnimator.GetComponentInParent<DriverCrashDetector>();
        _driverCinematicCam = myAnimator.GetComponent<DriverCinematicCam>();
        
        crashTimeout = false;
        cooldown = minimumCameraDuration;
        if (setCam)
        {
            _driverCinematicCam.SetCam(stateCam);
        }
        
    }
    
    
    
    // returns true if we detect we are gonna crash with the environment
    protected virtual bool Update_EnvironmentCrashDetectSensorActions()
    {
        if (crashTimeout)
        {
            currentSensorReport = _driverCrashDetector.currentSensorReport;
            // if we are about to crash into the environment, then set the trigger
            if (currentSensorReport.crashValue >= crashThreshold)
            {
                // look at the left/right stuff
                if (currentSensorReport.leftRightCoefficient < -0.5)
                {
                    myAnimator.SetTrigger(crashTriggerEnvironmentDetectLeft);
                }
                else if (currentSensorReport.leftRightCoefficient > 0.5)
                {
                    myAnimator.SetTrigger(crashTriggerEnvironmentDetectRight);
                }
                else
                {
                    myAnimator.SetTrigger(crashTriggerEnvironmentDetectAhead);
                }
            }
        }

        return false;
        // if in a few seconds, we no longer have the crash signal from the sensors, then go back (to do in about to crash state)
    }


    protected void Update_TimeoutTimer()
    {
        cooldown -= Time.deltaTime;
        if(cooldown<=0) SetCrashBackTimeout(true);
    }


    protected void SetCrashBackTimeout(bool set)
    {
        crashTimeout = set;
    }
    
    

    protected virtual bool Update_CrashDetectBack()
    {
        if (crashTimeout)
        {
            currentSensorReport = _driverCrashDetector.currentSensorReport;
            if (currentSensorReport.crashValue < crashThreshold)
            {
                myAnimator.SetTrigger(backTrigger);
            }
        }

        return false;
    }
    
    

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        // get the sensor report
        Update_EnvironmentCrashDetectSensorActions();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
