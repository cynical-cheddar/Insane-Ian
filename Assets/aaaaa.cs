using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class aaaaa : MonoBehaviour, ITriggerEnterEvent
{
    public bool requiresData { get { return true; } }

    void Start() {
        PhysXRigidBody rigidBody = GetComponent<PhysXRigidBody>();

        if (rigidBody != null) {
            rigidBody.centreOfMass = Vector3.forward;
        }
    }

    public void OnTriggerEnter() {

    }

    public void OnTriggerEnter(PhysXCollider collider) {
        Debug.Log("wop");
    }

    void Update() {
        PhysXRaycastHit hit = PhysXRaycast.GetRaycastHit();

        if (PhysXRaycast.Fire(transform.position, transform.forward, hit)) {
            Debug.Log(hit.point);
            Debug.Log(hit.normal);
            Debug.Log(hit.distance);
            Debug.Log(hit.collider.gameObject.name);
            Debug.Log(hit.transform.gameObject.name);
        }

        PhysXRaycast.ReleaseRaycastHit(hit);
    }
}
