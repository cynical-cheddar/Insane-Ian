using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wheelVisualiser : MonoBehaviour
{
    public GameObject wheelVis;
    public PhysXWheelCollider wheelCollider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        wheelVis.transform.position = wheelCollider.wheelCentre;
    }
}
