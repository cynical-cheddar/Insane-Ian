using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXSphereCollider : PhysXCollider
{
    public float radius = 0.5f;

    // Start is called before the first frame update
    public override void Setup(PhysXRigidBody attachedRigidBody)
    {
        IntPtr geom = PhysXLib.CreateSphereGeometry(radius);
        shape = PhysXLib.CreateShape(geom, physXMaterial);

        base.Setup(attachedRigidBody);
    }
}
