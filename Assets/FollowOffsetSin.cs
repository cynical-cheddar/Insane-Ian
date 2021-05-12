using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FollowOffsetSin : MonoBehaviour
{
    // Start is called before the first frame update
    CinemachineTransposer cam;

    public float amplitiude = 5f;
    void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTransposer>();
    }

    // Update is called once per frame
    void Update()
    {
        cam.m_FollowOffset.x = Mathf.Sin(Time.time * 0.5f) * amplitiude;
    }
}
