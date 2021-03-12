using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ActionReplayBehaviour : MonoBehaviour
{
    Camera cam;
    CinemachineBrain cinemachineBrain;
    CinemachineVirtualCamera virtualCamera;

    // Start is called before the first frame update
    void Start() {
        cam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            ToggleCamera();
        }
    }

    public void ToggleCamera() {
        cam.enabled = !cam.enabled;
    }
}
