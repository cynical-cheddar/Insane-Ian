using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXMeshCollider : PhysXCollider
{
    public Mesh mesh;

    // Start is called before the first frame update
    public override void Setup()
    {
        IntPtr vertexArray = PhysXLib.CreateMeshVertexArray();

        Vector3[] vertices = mesh.vertices;
        foreach (Vector3 vertex in vertices) {
            PhysXLib.AddVertexToArray(vertexArray, new PhysXVec3(vertex));
        }

        IntPtr geom = PhysXLib.CreateConvexMeshGeometry(vertexArray);
        shape = PhysXLib.CreateShape(geom, physXMaterial);

        base.Setup();
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}
