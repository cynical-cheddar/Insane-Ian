using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXMeshCollider : PhysXCollider
{
    public float x = 1, y = 1, z = 1;

    // Start is called before the first frame update
    public override void Setup()
    {
        IntPtr geom = PhysXLib.CreateBoxGeometry(x / 2, y / 2, z / 2);
        shape = PhysXLib.CreateShape(geom, physXMaterial);

        base.Setup();
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}
