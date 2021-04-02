using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class yeet : MonoBehaviour, ICollisionEnterEvent
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<PhysXRigidBody>().AddForce(Vector3.right * 10, ForceMode.Impulse);
    }

    public void OnCollisionEnter() {
        Debug.Log("boop!");
    }
}
