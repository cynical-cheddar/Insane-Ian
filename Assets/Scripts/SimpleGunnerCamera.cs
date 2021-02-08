using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class SimpleGunnerCamera : MonoBehaviour
{
    // Start is called before the first frame update
    public CinemachineVirtualCamera cam;
    private void OnEnable()
    {
        cam.enabled = true;
    }
}
