using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public int id;
    public Action<PooledObject> Finished;

    // A component reference for fast access -- avoids calls to GetComponent<>().
    private PhysXRigidBody rb;
    private TrailRenderer tr;

    void Awake()
    {
        // Debug.LogWarning("Pooled Object has not been ported to the new PhysX system");
        // return;
        if (GetComponent<PhysXRigidBody>() != null) rb = GetComponent<PhysXRigidBody>();
        if (GetComponentInChildren<TrailRenderer>() != null) {
            tr = GetComponentInChildren<TrailRenderer>();
        }
    }

    private void OnEnable()
    {
        if (tr!=null) tr.Clear();
    }

    public void Finish() {
        if (rb != null) rb.velocity = Vector3.zero;
        if (tr != null) tr.Clear();
        if (Finished != null) {
            Finished(this);
        }
    }

    // Convenience method to call finish when particles finish.
    // Needs ParticleSystem stop action to be set to "Callback".
    private void OnParticleSystemStopped() {
        Finish();
    }

    public void Finish(float t)
    {
        Invoke(nameof(Finish), t);
    }
}
