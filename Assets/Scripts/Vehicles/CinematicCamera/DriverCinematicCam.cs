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
    CinemachineBasicMultiChannelPerlin defaultCamCinemachineBasicMultiChannelPerlin;
    
    public CinemachineVirtualCamera environmentCrashLeft;
    public CinemachineVirtualCamera environmentCrashCentre;
    public CinemachineVirtualCamera environmentCrashRight;
    
    public CinemachineVirtualCamera environmentWarning1Left;
    public CinemachineVirtualCamera environmentWarning1Centre;
    public CinemachineVirtualCamera environmentWarning1Right;
    
    
    public CinemachineVirtualCamera carCrashFrontLeft;
    public CinemachineVirtualCamera carCrashFrontRight;
    
    public CinemachineVirtualCamera carCrashBackLeft;
    public CinemachineVirtualCamera carCrashBackRight;
    
    public enum Cams {defaultCamEnum, environmentCrashLeftEnum, environmentCrashCentreEnum, environmentCrashRightEnum,environmentWarning1LeftEnum, environmentWarning1CentreEnum, environmentWarning1RightEnum, carCrashFrontLeftEnum, carCrashFrontRightEnum, carCrashBackLeftEnum, carCrashBackRightEnum }

    [Serializable]
    public struct FovSpeedState
    {
        public float speed;
        public float fov;
    }

    public FovSpeedState minSpeedState;
    public FovSpeedState maxSpeedState;
    
    float shakeTimerMax = 0.5f;
    float shakeTimerCur = 0;

    float cameraShakeAmplitude = 1f;

    public void ShakeCams(float intensity, float time){
        shakeTimerMax = time;
        shakeTimerCur = 0;
        cameraShakeAmplitude = intensity;
        defaultCamCinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
    }

    private void Update() {
        shakeTimerCur += Time.deltaTime;
        if (shakeTimerCur <= shakeTimerMax)
        {
                defaultCamCinemachineBasicMultiChannelPerlin.m_AmplitudeGain =
                    Mathf.Lerp(cameraShakeAmplitude, 0f, (shakeTimerCur / shakeTimerMax));
        }

       speed = _driverCrashDetector.currentSensorReport.speed;
        SetCamFovs();
    }

    void Awake() {
        defaultCamCinemachineBasicMultiChannelPerlin = defaultCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

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
        environmentWarning1Left.m_LookAt = transform.root;
        environmentWarning1Centre.m_LookAt = transform.root;
        environmentWarning1Right.m_LookAt = transform.root;

    }

    public void ResetCam()
    {
        if (this.enabled)
        {
            SetCam(defaultCam);
            GetComponent<Animator>().Rebind();
            GetComponent<Animator>().enabled = true;
        }
    }

    public void SetCam(Cams cam)
    {
        if(this.enabled){
            if(cam == Cams.defaultCamEnum) SetCam(defaultCam);
            if(cam == Cams.environmentCrashLeftEnum) SetCam(environmentCrashLeft);
            if(cam == Cams.environmentCrashCentreEnum) SetCam(environmentCrashCentre);
            if(cam == Cams.environmentCrashRightEnum) SetCam(environmentCrashRight);
            if(cam == Cams.environmentWarning1LeftEnum) SetCam(environmentWarning1Left);
            if(cam == Cams.environmentWarning1CentreEnum) SetCam(environmentWarning1Centre);
            if(cam == Cams.environmentWarning1RightEnum) SetCam(environmentWarning1Right);
            if(cam == Cams.carCrashBackLeftEnum) SetCam(carCrashBackLeft);
            if(cam == Cams.carCrashBackRightEnum) SetCam(carCrashBackRight);
            if(cam == Cams.carCrashFrontLeftEnum) SetCam(carCrashFrontLeft);
            if(cam == Cams.carCrashFrontRightEnum) SetCam(carCrashFrontRight);
        }
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

    Transform GetCrashTarget()
    {
        Transform t = _driverCrashDetector.currentSensorReport.lastCrashedPlayer;
        return t;
    }

    public void SetCam(CinemachineVirtualCamera cam)
    {
        defaultCam.enabled = false;
        environmentCrashLeft.enabled = false;
        environmentCrashCentre.enabled = false;
        environmentCrashRight.enabled = false;
        
        environmentWarning1Left.enabled = false;
        environmentWarning1Centre.enabled = false;
        environmentWarning1Right.enabled = false;

        carCrashBackLeft.enabled = false;
        carCrashBackRight.enabled = false;
        carCrashFrontLeft.enabled = false;
        carCrashFrontRight.enabled = false;

        
        
        if (cam == carCrashBackLeft) carCrashBackLeft.m_LookAt = GetCrashTarget();
        if (cam == carCrashBackRight) carCrashBackRight.m_LookAt = transform.root;
        if (cam == carCrashFrontLeft) carCrashFrontLeft.m_LookAt = GetCrashTarget();
        if (cam == carCrashFrontRight) carCrashFrontRight.m_LookAt = GetCrashTarget();
        
        

        /*
        if (cam == carCrashBackLeft) carCrashBackLeft.m_LookAt = transform.root;
        if (cam == carCrashBackRight) carCrashBackRight.m_LookAt = transform.root;
        if (cam == carCrashFrontLeft) carCrashFrontLeft.m_LookAt = transform.root;
        if (cam == carCrashFrontRight) carCrashFrontRight.m_LookAt = transform.root;
        */
        
        cam.enabled = true;
    }

}
