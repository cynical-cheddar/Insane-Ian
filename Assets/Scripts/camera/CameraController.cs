using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[VehicleScript(ScriptType.playerDriverScript)]
public class CameraController : MonoBehaviour
{
    public GameObject focus;
    private Rigidbody rb;

    private bool isFixed = true;
    private bool isReverseCam = false;

    private bool isChanged = false;

    public GameObject driveCamGO;
    public GameObject driveCamRevGO;
    public GameObject driveCamFixGO;
    public GameObject driveCamFixRevGO;

    private CinemachineFreeLook driveCam;
    private CinemachineFreeLook driveCamRev;
    private CinemachineFreeLook driveCamFix;
    private CinemachineFreeLook driveCamFixRev;


    // Start is called before the first frame update
    void Start()
    {
        rb = focus.GetComponent <Rigidbody> ();
        driveCam = driveCamGO.GetComponent<CinemachineFreeLook>();
        driveCamRev = driveCamRevGO.GetComponent<CinemachineFreeLook>();
        driveCamFix = driveCamFixGO.GetComponent<CinemachineFreeLook>();
        driveCamFixRev = driveCamFixRevGO.GetComponent<CinemachineFreeLook>();

        driveCam.enabled = true;
        driveCamRev.enabled = true;
        driveCamFix.enabled = true;
        driveCamFixRev.enabled = true;

        driveCam.Priority = 0;
        driveCamRev.Priority = 0;
        driveCamFix.Priority = 1;
        driveCamFixRev.Priority = 0;
        driveCam.LookAt = focus.transform;
        driveCam.Follow = focus.transform;
        driveCamRev.LookAt = focus.transform;
        driveCamRev.Follow = focus.transform;
        driveCamFix.LookAt = focus.transform;
        driveCamFix.Follow = focus.transform;
        driveCamFixRev.LookAt = focus.transform;
        driveCamFixRev.Follow = focus.transform;
        driveCamGO.SetActive(false);
        driveCamRevGO.SetActive(false);
        driveCamFixGO.SetActive(true);
        driveCamFixRevGO.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            isFixed = !isFixed;
            isChanged = true;
        }
        Vector3 velocity = rb.velocity;
        Vector3 localVel = rb.transform.InverseTransformDirection(velocity);
        if (Input.GetKeyDown("s") && localVel.z < 0 && !isReverseCam)
        {
            isReverseCam = true;
            isChanged = true;
        }
        if (Input.GetKeyDown("w") && localVel.z >= 0 && isReverseCam)
        {
            isReverseCam = false;
            isChanged = true;
        }
        if (isChanged)
        {
            if (isFixed)
            {
                if (isReverseCam)
                {
                    driveCam.Priority = 0;
                    driveCamRev.Priority = 0;
                    driveCamFix.Priority = 0;
                    driveCamFixRev.Priority = 1;
                    driveCamGO.SetActive(false);
                    driveCamRevGO.SetActive(false);
                    driveCamFixGO.SetActive(false);
                    driveCamFixRevGO.SetActive(true);
                    driveCamFixRev.m_YAxis.Value = driveCamRev.m_YAxis.Value;
                }
                else
                {
                    driveCam.Priority = 0;
                    driveCamRev.Priority = 0;
                    driveCamFix.Priority = 1;
                    driveCamFixRev.Priority = 0;
                    driveCamGO.SetActive(false);
                    driveCamRevGO.SetActive(false);
                    driveCamFixGO.SetActive(true);
                    driveCamFixRevGO.SetActive(false);
                    driveCamFix.m_YAxis.Value = driveCam.m_YAxis.Value;
                }
            }
            else
            {
                if (isReverseCam)
                {
                    driveCam.Priority = 0;
                    driveCamRev.Priority = 1;
                    driveCamFix.Priority = 0;
                    driveCamFixRev.Priority = 0;
                    driveCamGO.SetActive(false);
                    driveCamRevGO.SetActive(true);
                    driveCamFixGO.SetActive(false);
                    driveCamFixRevGO.SetActive(false);
                    driveCamRev.m_XAxis.Value = 180;
                    driveCamRev.m_YAxis.Value = driveCamFixRev.m_YAxis.Value;
                }
                else
                {
                    driveCam.Priority = 1;
                    driveCamRev.Priority = 0;
                    driveCamFix.Priority = 0;
                    driveCamFixRev.Priority = 0;
                    driveCamGO.SetActive(true);
                    driveCamRevGO.SetActive(false);
                    driveCamFixGO.SetActive(false);
                    driveCamFixRevGO.SetActive(false);
                    driveCam.m_XAxis.Value = 0;
                    driveCam.m_YAxis.Value = driveCamFix.m_YAxis.Value;
                }
            }
        }
        isChanged = false;
    }
}
