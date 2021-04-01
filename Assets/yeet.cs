using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class yeet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<PhysXRigidBody>().AddForce(Vector3.right * 10, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
