using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXMeshCollider : PhysXCollider
{
    public Mesh mesh;
    public bool convex = true;

    // Start is called before the first frame update
    public override void Setup()
    {
        IntPtr vertexArray = PhysXLib.CreateVectorArray();

        Vector3[] vertices = mesh.vertices;
        foreach (Vector3 vertex in vertices) {
            PhysXLib.AddVectorToArray(vertexArray, new PhysXVec3(vertex));
        }

        IntPtr geom = IntPtr.Zero;
        if (convex) {
            geom = PhysXLib.CreateConvexMeshGeometry(vertexArray);
        }
        else {
            geom = PhysXLib.CreateMeshGeometry(vertexArray, mesh.triangles, mesh.triangles.Length);
        }

        shape = PhysXLib.CreateShape(geom, physXMaterial);

        base.Setup();
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}
