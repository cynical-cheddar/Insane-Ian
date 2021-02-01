using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using Photon.Pun;
using UnityEngine.Experimental.GlobalIllumination;

public class CameraLookController : MonoBehaviourPunCallbacks
{
    //public List<CinemachineVirtualCameraBase> CameraBases;
    public Transform vehicleTransform;
    public bool checkForPhotonView = true;
    public GameObject freelookCamGameObject;
    private GameObject freelookcamInstance;
    bool panning = false;
    Transform selectedVehicle;
    Transform lookpoint;
    public CinemachineFreeLook freeLookCam;
    int mouseButton = 1;
    public float speedX;
    public float speedY;

    public bool directControl = true;
    
    

    public float scrollSensitivity = 1f;

    float lastZoomLevel = 0.5f;

    float curscroll;



    [SerializeField]
   // [Range(0.0f, 1f)]
    public float zoomLevel = 0.5f;

    float targetZoomLevel = 0.5f;
    

    float strategicZoomLevel = 0.0f;
    public Transform getLookPoint(){
        if(lookpoint!=null)return lookpoint;
        else return selectedVehicle;
    }
    public void setDirectControlBool(bool set){
        directControl = set;
    }
    public OrbitDetails[] m_OrbitsFar = new OrbitDetails[3] 
    { 
     // These are the default orbits
        new OrbitDetails(20f, 60f),
        new OrbitDetails(0f, 60f),
        new OrbitDetails(-20f, 60f)
    };

    public OrbitDetails[] m_OrbitsNear = new OrbitDetails[3] 
    { 
     // These are the default orbits
        new OrbitDetails(20f, 10f),
        new OrbitDetails(0f, 10f),
        new OrbitDetails(-20f, 10f)
    };
    
    
     [Serializable]
     public struct OrbitDetails 
        { 
            /// <summary>Height relative to target</summary>
            public float m_Height; 
            /// <summary>Radius of orbit</summary>
            public float m_Radius; 
            /// <summary>Constructor with specific values</summary>
            public OrbitDetails(float h, float r) { m_Height = h; m_Radius = r; }
    }
   
    private void Start()
    {
        if (vehicleTransform == null) vehicleTransform = transform;
        Debug.Log("spawning camera rig");
            freelookcamInstance = Instantiate(freelookCamGameObject, transform.position, transform.rotation);
            freeLookCam = freelookcamInstance.GetComponent<CinemachineFreeLook>();
            freeLookCam.m_Follow = vehicleTransform;
            freeLookCam.m_LookAt = vehicleTransform;

            if (photonView.IsMine== false)
            {
                freeLookCam.enabled = false;
            }


    }
    


    void selectCamera(CinemachineVirtualCameraBase cam){
        speedX = freeLookCam.m_XAxis.m_MaxSpeed;
        speedY = freeLookCam.m_YAxis.m_MaxSpeed;
    }
    void Update()
    {
        if(directControl){
        // Rotation
            if(Input.GetMouseButton(mouseButton)){
                /*Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;*/
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    
                freeLookCam.m_XAxis.m_MaxSpeed = speedX;
                freeLookCam.m_YAxis.m_MaxSpeed = speedY;
            }
            else{
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                freeLookCam.m_XAxis.m_MaxSpeed = 0;
                freeLookCam.m_YAxis.m_MaxSpeed = 0;
            }
            // Zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            // create a target zoom level. Add scroll to it
            targetZoomLevel -= scroll * scrollSensitivity;
            if(targetZoomLevel > 1) targetZoomLevel = 1;
            if(targetZoomLevel < 0) targetZoomLevel  =0;
            // lerp between currentzoom and target zoom
            zoomLevel = Mathf.Lerp(zoomLevel, targetZoomLevel, Time.unscaledDeltaTime * scrollSensitivity * 4);
            
            if(targetZoomLevel != zoomLevel){
                // calculate new orbit radius

               // zoomLevel -= scroll;
                if(zoomLevel < 0)zoomLevel = 0f;
                else if(zoomLevel >= 1)
                {
                    zoomLevel = 1;
                }


                freeLookCam.m_Orbits = new CinemachineFreeLook.Orbit[3] 
                    { 
                        // These are the default orbits
                        new CinemachineFreeLook.Orbit(Mathf.Lerp(m_OrbitsNear[0].m_Height, m_OrbitsFar[0].m_Height, zoomLevel), Mathf.Lerp(m_OrbitsNear[0].m_Radius, m_OrbitsFar[0].m_Radius, zoomLevel)),
                        new CinemachineFreeLook.Orbit(Mathf.Lerp(m_OrbitsNear[1].m_Height, m_OrbitsFar[1].m_Height, zoomLevel), Mathf.Lerp(m_OrbitsNear[1].m_Radius, m_OrbitsFar[1].m_Radius, zoomLevel)),
                        new CinemachineFreeLook.Orbit(Mathf.Lerp(m_OrbitsNear[2].m_Height, m_OrbitsFar[2].m_Height, zoomLevel), Mathf.Lerp(m_OrbitsNear[2].m_Radius, m_OrbitsFar[2].m_Radius, zoomLevel))
                    };
            }
        }

            
            lastZoomLevel = zoomLevel;
        }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        // if we are in strategic camera mode, allow panning
            
    }
}

