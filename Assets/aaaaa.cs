using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class aaaaa : MonoBehaviour, ITriggerEnterEvent
{
    private float timer = 0;

    public bool requiresData { get { return true; } }

    private PhysXRigidBody body;
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start() {
        body = GetComponent<PhysXRigidBody>();
        startPosition = body.position;
        startRotation = body.rotation;
        body.AddTorque(Random.onUnitSphere, ForceMode.Impulse);
    }

    public void OnTriggerEnter() {

    }

    public void OnTriggerEnter(PhysXCollider collider) {
        Debug.Log("wop");
    }

    void FixedUpdate() {
        timer += Time.deltaTime;

        if (timer >= 1) {
            body.position = startPosition;
            body.rotation = startRotation;
            timer = 0;
        }
    }
}
