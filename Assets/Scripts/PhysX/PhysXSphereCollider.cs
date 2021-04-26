using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXSphereCollider : PhysXCollider
{
    public float radius = 0.5f;

    // Start is called before the first frame update
    public override void Setup(PhysXBody attachedRigidBody, uint vehicleId)
    {
        IntPtr geom = PhysXLib.CreateSphereGeometry(radius);
        shape = PhysXLib.CreateShape(geom, physXMaterial, 0.02f);

        base.Setup(attachedRigidBody, vehicleId);
    }

    void OnDrawGizmosSelected() {
        Vector3 oldScale = transform.localScale;
        transform.localScale = Vector3.one;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(offset, radius);
        Gizmos.matrix = Matrix4x4.identity;
        transform.localScale = oldScale;
    }
}
