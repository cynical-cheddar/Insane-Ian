using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXSphereCollider : PhysXCollider
{
    public float radius = 0.5f;

    // Start is called before the first frame update
    public override void Setup(PhysXBody attachedRigidBody)
    {
        IntPtr geom = PhysXLib.CreateSphereGeometry(radius);
        shape = PhysXLib.CreateShape(geom, physXMaterial, 0.02f);

        base.Setup(attachedRigidBody);
    }
}
