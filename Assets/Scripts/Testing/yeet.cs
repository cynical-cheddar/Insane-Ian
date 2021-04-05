using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class yeet : MonoBehaviour, ICollisionEnterEvent
{
    public bool requiresData { get { return false; } }

    void Start()
    {
        GetComponent<PhysXRigidBody>().AddForce(Vector3.right * 10, ForceMode.Impulse);
    }

    public void OnCollisionEnter() {
        Debug.Log("boop!");
    }

    public void OnCollisionEnter(PhysXCollision collision) {
        Debug.LogError("ohno");
    }
}
