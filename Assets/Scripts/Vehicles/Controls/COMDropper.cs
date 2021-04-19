// Expose center of mass to allow it to be set from
// the inspector.
using UnityEngine;
using System.Collections;

public class COMDropper : MonoBehaviour {
    public Rigidbody rb;
    public Vector3 Shift;

    void Start() {
        Debug.LogWarning("COM Dropper has not been ported to the new PhysX system");
        return;

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Shift;
    }

    private void OnDrawGizmos() {
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(transform.position + transform.rotation * Shift, 1f);
    }
}