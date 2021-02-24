using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[VehicleScript(ScriptType.playerDriverScript)]
public class CameraController : MonoBehaviour
{
    enum CamType {
        Reg = 0,
        Rev = 1,
        FixReg = 2,
        FixRev = 3
    }

    public GameObject focus;
    private Rigidbody rb;

    private bool isFixed = false;
    private bool isReverseCam = false;

    private bool isChanged = false;

    public List<CinemachineFreeLook> cameras;

    void ActivateCamera(CamType camType) {
        foreach (CinemachineFreeLook camera in cameras) {
            camera.Priority = 0;
            camera.enabled = false;
        }
        cameras[(int)camType].Priority = 1;
        cameras[(int)camType].enabled = true;
    }

    // Start is called before the first frame update
    void Start() {
        rb = focus.GetComponent<Rigidbody>();

        ActivateCamera(CamType.Reg);

        foreach (CinemachineFreeLook camera in cameras) {
            camera.LookAt = focus.transform;
            camera.Follow = focus.transform;
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            isFixed = !isFixed;
            isChanged = true;
        }
        Vector3 velocity = rb.velocity;
        Vector3 localVel = rb.transform.InverseTransformDirection(velocity);
        if (Input.GetKey(KeyCode.S) && localVel.z < 0 && !isReverseCam) {
            isReverseCam = true;
            isChanged = true;
        }
        if (Input.GetKey(KeyCode.W) && localVel.z >= 0 && isReverseCam) {
            isReverseCam = false;
            isChanged = true;
        }
        if (isChanged) {
            if (isFixed) {
                if (isReverseCam) {
                    ActivateCamera(CamType.FixRev);
                    cameras[(int)CamType.FixRev].m_YAxis.Value = cameras[(int)CamType.Rev].m_YAxis.Value;
                } else {
                    ActivateCamera(CamType.FixReg);
                    cameras[(int)CamType.FixReg].m_YAxis.Value = cameras[(int)CamType.Reg].m_YAxis.Value;
                }
            } else {
                if (isReverseCam) {
                    ActivateCamera(CamType.Rev);
                    cameras[(int)CamType.Rev].m_XAxis.Value = 180;
                    cameras[(int)CamType.Rev].m_YAxis.Value = cameras[(int)CamType.FixRev].m_YAxis.Value;
                } else {
                    ActivateCamera(CamType.Reg);
                    cameras[(int)CamType.Reg].m_XAxis.Value = 0;
                    cameras[(int)CamType.Reg].m_YAxis.Value = cameras[(int)CamType.FixReg].m_YAxis.Value;
                }
            }
        }
        isChanged = false;
    }
}
