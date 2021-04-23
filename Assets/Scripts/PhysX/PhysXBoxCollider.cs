using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXBoxCollider : PhysXCollider
{
    public float x = 1, y = 1, z = 1;

    // Start is called before the first frame update
    public override void Setup(PhysXBody attachedRigidBody, uint vehicleId)
    {
        IntPtr geom = PhysXLib.CreateBoxGeometry(x / 2, y / 2, z / 2);
        shape = PhysXLib.CreateShape(geom, physXMaterial, 0.02f);

        base.Setup(attachedRigidBody, vehicleId);
    }

    void OnDrawGizmosSelected() {
        Vector3 oldScale = transform.localScale;
        transform.localScale = Vector3.one;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(offset, new Vector3(x, y, z));
        Gizmos.matrix = Matrix4x4.identity;
        transform.localScale = oldScale;
    }
}
