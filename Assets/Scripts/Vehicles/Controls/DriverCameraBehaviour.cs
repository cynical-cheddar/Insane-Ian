using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;
using Gamestate;

[VehicleScript(ScriptType.playerDriverScript)]
public class DriverCameraBehaviour : MonoBehaviour, IPunInstantiateMagicCallback {
    CinemachineFreeLook cam;
    CinemachineVirtualCamera firstPersonCam;
    GamestateTracker gamestateTracker;
    VehicleManager vehicleManager;
    Transform thirdPersonFocus;


    public bool lockCursorToWindow = true;

    bool isFirstPerson = false;
    int teamId;


    // Start is called before the first frame update
    void Start() {
        if (lockCursorToWindow && FindObjectOfType<PlinthManager>() == null) Cursor.lockState = CursorLockMode.Locked;
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        cam = GetComponentInChildren<CinemachineFreeLook>();
        firstPersonCam = GetComponentInChildren<CinemachineVirtualCamera>();
        firstPersonCam.enabled = false;
        vehicleManager = GetComponentInParent<VehicleManager>();
        thirdPersonFocus = cam.LookAt;
        // Give the vehicle manager time to assign teamIds
        //Invoke(nameof(AssignPriority), 3f);
        AssignPriority();
    }

    void AssignPriority() {
        PlayerEntry entry = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        int teamId = entry.teamId;
        entry.Release();

        if (teamId == vehicleManager.teamId) {
            cam.enabled = true;
            cam.Priority = 100;
        }

    }

    // Update is called once per frame
    void Update() {
        // Scroll camera in/out (locked in 1st person)
        if (!isFirstPerson) cam.m_YAxis.Value -= Input.mouseScrollDelta.y * 0.1f;

        if (Input.GetKeyDown(KeyCode.C) && !isFirstPerson) {
            // Reverse camera
            cam.m_XAxis.m_MinValue = 90;
            cam.m_XAxis.m_MaxValue = 270;
            cam.m_XAxis.Value = 180;
        }

        if (Input.GetKeyUp(KeyCode.C) && !isFirstPerson) {
            // Regular camera
            cam.m_XAxis.m_MinValue = -90;
            cam.m_XAxis.m_MaxValue = 90;
            cam.m_XAxis.Value = 0;
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            isFirstPerson = !isFirstPerson;
            if (isFirstPerson) {
                /*cam.m_BindingMode = CinemachineTransposer.BindingMode.LockToTarget;
                cam.m_XAxis.m_InputAxisName = "";
                cam.m_XAxis.m_MinValue = 180;
                cam.m_XAxis.m_MaxValue = 180;
                cam.m_YAxis.Value = 0;
                cam.LookAt = null;*/
                firstPersonCam.enabled = true;
                cam.enabled = false;

            } else {
                /*cam.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
                cam.m_XAxis.m_InputAxisName = "Mouse X";
                cam.m_XAxis.m_MinValue = -90;
                cam.m_XAxis.m_MaxValue = 90;
                cam.m_XAxis.Value = 0;
                cam.LookAt = thirdPersonFocus;*/
                firstPersonCam.enabled = false;
                cam.enabled = true;
            }
        }
    }

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info) {

    }
}
