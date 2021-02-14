using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerGunnerController : MonoBehaviour
{
    new public CinemachineVirtualCamera camera;
    public float cameraSensitivity = 1;
    private TurretController turretController;

    // Start is called before the first frame update
    void Start()
    {
        turretController = GetComponent<TurretController>();
    }

     void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        transform.parent = transform.parent.parent;
        camera.enabled = true;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        camera.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        turretController.ChangeTargetYaw(cameraSensitivity * Input.GetAxis("Mouse X") * Time.fixedDeltaTime);
        turretController.ChangeTargetPitch(-(cameraSensitivity * Input.GetAxis("Mouse Y") * Time.fixedDeltaTime));
    }
}
