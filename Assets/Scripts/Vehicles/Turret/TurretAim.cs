using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAim : MonoBehaviour
{
    private float pitch = 0;
    private float yaw = 0;
    public float cameraSensitivity = 1;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        yaw += cameraSensitivity * Input.GetAxis("Mouse X");
        pitch -= cameraSensitivity * Input.GetAxis("Mouse Y");
        transform.eulerAngles = new Vector3(Mathf.Clamp(pitch, -45, 45), yaw, 0f);
    }
}
