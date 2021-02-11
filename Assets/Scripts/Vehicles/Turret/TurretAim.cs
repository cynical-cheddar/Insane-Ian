using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TurretAim : MonoBehaviour
{
    private float pitch = 0;
    private float yaw = 0;
    public float cameraSensitivity = 1;
    public float upTraverse = 75;
    public float downTraverse = 30;
    public CinemachineVirtualCamera camera;


    // Start is called before the first frame update
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
        yaw += cameraSensitivity * Input.GetAxis("Mouse X") * Time.fixedDeltaTime;
        pitch -= cameraSensitivity * Input.GetAxis("Mouse Y") * Time.fixedDeltaTime;
        pitch = Mathf.Clamp(pitch, -45, 45);
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        float localPitch = transform.localEulerAngles.x;
        if (localPitch > 180) localPitch -= 360;
        transform.localRotation = Quaternion.Euler(Mathf.Clamp(localPitch, -upTraverse, downTraverse), transform.localEulerAngles.y, transform.localEulerAngles.z);
    }
}
