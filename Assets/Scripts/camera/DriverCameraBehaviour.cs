using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class DriverCameraBehaviour : MonoBehaviour
{
    CinemachineFreeLook cam;
    GamestateTracker gamestateTracker;
    VehicleManager vehicleManager;

    public bool lockCursorToWindow = true;

    // Start is called before the first frame update
    void Start() {
        if (lockCursorToWindow) Cursor.lockState = CursorLockMode.Locked;
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        cam = GetComponent<CinemachineFreeLook>();
        vehicleManager = GetComponentInParent<VehicleManager>();
        // Give the vehicle manager time to assign teamIds.
        Invoke(nameof(AssignPriority), 0.2f);
    }

    void AssignPriority() {
        if (gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber).teamId == vehicleManager.teamId) {
            cam.Priority = 1;
        }
    }

    // Update is called once per frame
    void Update() {
        cam.m_YAxis.Value -= Input.mouseScrollDelta.y * 0.1f;
    }
}
