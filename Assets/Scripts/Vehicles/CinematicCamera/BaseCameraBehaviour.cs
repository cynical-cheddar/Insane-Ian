using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class BaseCameraBehaviour : StateMachineBehaviour
{
    public float backTimeoutTime = 1f;
    public float minimumStateTime = 0f;
    public bool setCam = true;
    public DriverCinematicCam.Cams stateCam;
    
    protected DriverCrashDetector _driverCrashDetector;
    protected DriverCinematicCam _driverCinematicCam;
    protected DriverCinematicCamBehaviourDictionary _driverCinematicCamBehaviourDictionary;
    protected DriverCrashDetector.CurrentSensorReportStruct currentSensorReport;
    protected Animator myAnimator;
    public float timeToCrashThreshold = 0.2f;
    
    // crash into environment 
    public string crashTriggerEnvironmentAhead = "suddenEnvironmentCrashAhead";
    public string crashTriggerEnvironmentLeft = "suddenEnvironmentCrashLeft";
    public string crashTriggerEnvironmentRight = "suddenEnvironmentCrashRight";
    
    
    public string crashTriggerEnvironmentDetectAhead = "environmentCrashAhead";
    public string crashTriggerEnvironmentDetectLeft = "environmentCrashLeft";
    public string crashTriggerEnvironmentDetectRight = "environmentCrashRight";

    public string crashTriggerRightImmediate = "crashFrontRight";
    public string crashTriggerLeftImmediate = "crashFrontLeft";
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
    protected float lockCooldown = 2f;

    protected bool locked = true;
    
    
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
        locked = true;
        crashTimeout = false;
        cooldown = backTimeoutTime;
        lockCooldown = minimumStateTime;
        if (setCam)
        {
            _driverCinematicCam.SetCam(stateCam);
        }
        
    }

    protected virtual void Update_CheckForCrash()
    {
        if (!locked)
        {
            currentSensorReport = _driverCrashDetector.currentSensorReport;
            if (currentSensorReport.crashed)
            {
                if (currentSensorReport.leftRightCoefficient <= 0)
                {
                    myAnimator.SetTrigger(crashTriggerLeftImmediate);
                    
                }
                else if (currentSensorReport.leftRightCoefficient > 0)
                {
                    myAnimator.SetTrigger(crashTriggerRightImmediate);
                    
                }
            }
        }
    }
    
    // returns true if we detect we are gonna crash with the environment
    protected virtual bool Update_EnvironmentCrashDetectSensorActions()
    {
        if (!locked)
        {
            currentSensorReport = _driverCrashDetector.currentSensorReport;
            // if we are about to crash into the environment, then set the trigger
            if (currentSensorReport.estimatedTimeToHit <= timeToCrashThreshold)
            {
                // look at crash distance


                // look at the left/right stuff
                if (currentSensorReport.leftRightCoefficient <= 0)
                {
                    myAnimator.SetTrigger(crashTriggerEnvironmentDetectLeft);
                    return true;
                }
                else if (currentSensorReport.leftRightCoefficient > 0)
                {
                    myAnimator.SetTrigger(crashTriggerEnvironmentDetectRight);
                    return true;
                }
                else
                {
                    myAnimator.SetTrigger(crashTriggerEnvironmentDetectAhead);
                    return true;
                }
            }
        }


        return false;
        // if in a few seconds, we no longer have the crash signal from the sensors, then go back (to do in about to crash state)
    }
    
    protected virtual bool Update_EnvironmentCrashCloseSensorActions()
    {
        if (!locked)
        {
            currentSensorReport = _driverCrashDetector.currentSensorReport;
            // if we are about to crash into the environment, then set the trigger
            if (currentSensorReport.estimatedTimeToHit <= timeToCrashThreshold)
            {
                // look at crash distance


                // look at the left/right stuff
                if (currentSensorReport.leftRightCoefficient < -0.5)
                {
                    myAnimator.SetTrigger(crashTriggerEnvironmentLeft);
                    return true;
                }
                else if (currentSensorReport.leftRightCoefficient > 0.5)
                {
                    myAnimator.SetTrigger(crashTriggerEnvironmentRight);
                    return true;
                }
                else
                {
                    myAnimator.SetTrigger(crashTriggerEnvironmentAhead);
                    return true;
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
    protected void Update_Lock()
    {
        lockCooldown -= Time.deltaTime;
        if(lockCooldown<=0) SetLock(false);
    }


    protected void SetLock(bool set)
    {
        locked = set;
    }
    
    protected void SetCrashBackTimeout(bool set)
    {
        crashTimeout = set;
    }
    
    

    protected virtual bool Update_CrashDetectBack()
    {
        if (crashTimeout && !locked)
        {
            currentSensorReport = _driverCrashDetector.currentSensorReport;
            if (currentSensorReport.estimatedTimeToHit > timeToCrashThreshold)
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
