using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[VehicleScript(ScriptType.playerDriverScript)]

public class DriverCinematicCam : MonoBehaviour
{
    
    
    private float speed = 0;
    private float fov = 70;
    private DriverCrashDetector _driverCrashDetector;
    
    public CinemachineVirtualCamera defaultCam;
    
    public CinemachineVirtualCamera environmentCrashLeft;
    public CinemachineVirtualCamera environmentCrashCentre;
    public CinemachineVirtualCamera environmentCrashRight;
    
    
    public enum Cams {defaultCamEnum, environmentCrashLeftEnum, environmentCrashCentreEnum, environmentCrashRightEnum}

    [Serializable]
    public struct FovSpeedState
    {
        public float speed;
        public float fov;
    }

    public FovSpeedState minSpeedState;
    public FovSpeedState maxSpeedState;
    
    void Start()
    {
        SetCam(defaultCam);
        GetComponent<Animator>().enabled = true;
        _driverCrashDetector = GetComponentInParent<DriverCrashDetector>();
        
        // set default cam follow targets
        defaultCam.m_LookAt = transform.root;
        environmentCrashLeft.m_LookAt = transform.root;
        environmentCrashCentre.m_LookAt = transform.root;
        environmentCrashRight.m_LookAt = transform.root;

    }

    public void ResetCam()
    {
        SetCam(defaultCam);
        GetComponent<Animator>().Rebind();
        GetComponent<Animator>().enabled = true;
    }

    public void SetCam(Cams cam)
    {
        if(cam == Cams.defaultCamEnum) SetCam(defaultCam);
        if(cam == Cams.environmentCrashLeftEnum) SetCam(environmentCrashLeft);
        if(cam == Cams.environmentCrashCentreEnum) SetCam(environmentCrashCentre);
        if(cam == Cams.environmentCrashRightEnum) SetCam(environmentCrashRight);

    }

    void SetCamFovs()
    {
        if (speed < minSpeedState.speed) fov = minSpeedState.fov;
        
        else if (speed > maxSpeedState.speed) fov = maxSpeedState.fov;

        else
        {
            // interpolate length
            fov = minSpeedState.fov + (speed - minSpeedState.speed) *
                ((maxSpeedState.fov - minSpeedState.fov) / (maxSpeedState.speed - minSpeedState.speed));
        }

        defaultCam.m_Lens.FieldOfView = fov;

    }

    public void SetCam(CinemachineVirtualCamera cam)
    {
        defaultCam.enabled = false;
        environmentCrashLeft.enabled = false;
        environmentCrashCentre.enabled = false;
        environmentCrashRight.enabled = false;
        


        cam.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        speed = _driverCrashDetector.currentSensorReport.speed;
        SetCamFovs();
    }
}
