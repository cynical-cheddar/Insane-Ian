using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXMeshCollider : PhysXCollider
{
    public Mesh mesh;
    public bool convex = true;

    // Start is called before the first frame update
    public override void Setup(PhysXBody attachedRigidBody)
    {
        IntPtr vertexArray = PhysXLib.CreateVectorArray();

        Vector3[] vertices = mesh.vertices;
        Vector3 centre = Vector3.zero;
        foreach (Vector3 vertex in vertices) {
            centre += vertex;
        }
        centre /= vertices.Length;

        offset += centre;

        foreach (Vector3 vertex in vertices) {
            PhysXLib.AddVectorToArray(vertexArray, new PhysXVec3(vertex - centre));
        }

        IntPtr geom = IntPtr.Zero;
        if (convex) {
            geom = PhysXLib.CreateConvexMeshGeometry(vertexArray);
        }
        else {
            geom = PhysXLib.CreateMeshGeometry(vertexArray, mesh.triangles, mesh.triangles.Length);
        }

        shape = PhysXLib.CreateShape(geom, physXMaterial, 0.02f);


        base.Setup(attachedRigidBody);
    }
}
