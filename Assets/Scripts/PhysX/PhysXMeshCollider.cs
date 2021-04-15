using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXMeshCollider : PhysXCollider
{
    public Mesh mesh = null;
    public bool convex = true;
    public Vector3 scale = Vector3.one;

    // Start is called before the first frame update
    public override void Setup(PhysXBody attachedRigidBody, uint vehicleId)
    {
        if (mesh == null) {
            Debug.LogError("Collider mesh is null on " + gameObject.name);
        }
        else if (!mesh.isReadable) {
            Debug.LogError("Collider mesh: " + mesh.name + " is not readable");
        }
        else {
            IntPtr vertexArray = PhysXLib.CreateVectorArray();

            Vector3[] unscaledVertices = mesh.vertices;
            Vector3[] vertices = new Vector3[unscaledVertices.Length];
            for (int i = 0; i < unscaledVertices.Length; i++) {
                vertices[i] = new Vector3(unscaledVertices[i].x * scale.x, unscaledVertices[i].y * scale.y, unscaledVertices[i].z * scale.z);
            }

            Vector3 centre = Vector3.zero;
            foreach (Vector3 vertex in vertices) {
                centre += vertex;
            }
            centre /= vertices.Length;

            offset += centre;

            foreach (Vector3 vertex in vertices) {
                PhysXLib.AddVectorToArray(vertexArray, new PhysXVec3((vertex - centre)));
            }

            IntPtr geom = IntPtr.Zero;
            if (convex) {
                geom = PhysXLib.CreateConvexMeshGeometry(vertexArray);
            }
            else {
                geom = PhysXLib.CreateMeshGeometry(vertexArray, mesh.triangles, mesh.triangles.Length);
            }

            shape = PhysXLib.CreateShape(geom, physXMaterial, 0.02f);


            base.Setup(attachedRigidBody, vehicleId);
        }
    }
}
