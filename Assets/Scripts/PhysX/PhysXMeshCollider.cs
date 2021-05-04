using System;
using System.Collections.Generic;
using UnityEngine;
using PhysX;

public class PhysXMeshCollider : PhysXCollider
{
    public Mesh mesh = null;
    public bool convex = true;
    public Vector3 scale = Vector3.one;
    private Mesh meshMesh = null;

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
            meshMesh = Instantiate(mesh);
            meshMesh.Clear();
            List<Vector3> meshMeshVertices = new List<Vector3>();

            IntPtr vertexArray = PhysXLib.CreateVectorArray();

            Vector3[] unscaledVertices = mesh.vertices;
            Vector3[] vertices = new Vector3[unscaledVertices.Length];
            for (int i = 0; i < unscaledVertices.Length; i++) {
                // vertices[i] = new Vector3(unscaledVertices[i].x * scale.x, unscaledVertices[i].y * scale.y, unscaledVertices[i].z * scale.z);
                vertices[i] = unscaledVertices[i];//new Vector3(unscaledVertices[i].x * scale.x, unscaledVertices[i].y * scale.y, unscaledVertices[i].z * scale.z);
            }

            Vector3 centre = Vector3.zero;
            foreach (Vector3 vertex in vertices) {
                centre += vertex;
            }
            centre /= vertices.Length;

            offset += new Vector3(centre.x * scale.x, centre.y * scale.y, centre.z * scale.z);

            PhysXVec3 physXVertex = new PhysXVec3(Vector3.zero);
            foreach (Vector3 vertex in vertices) {
                physXVertex.FromVector(vertex - centre);
                PhysXLib.AddVectorToArray(vertexArray, physXVertex);
            }

            IntPtr geom = IntPtr.Zero;
            if (convex) {
                geom = PhysXLib.CreateConvexMeshGeometry(vertexArray, new PhysXVec3(scale));
            }
            else {
                geom = PhysXLib.CreateMeshGeometry(vertexArray, mesh.triangles, mesh.triangles.Length / 3, new PhysXVec3(scale));
            }

            shape = PhysXLib.CreateShape(geom, physXMaterial, 0.02f);

            if (!convex) {
                int vertexCount = PhysXLib.GetMeshVertexCount(geom);
                int triCount = PhysXLib.GetMeshTriangleCount(geom);
                IntPtr usedVertices = PhysXLib.CreateVectorArray();
                int[] usedTris = new int[triCount * 3];
                PhysXLib.GetMeshGeometry(geom, usedVertices, usedTris);

                for (int i = 0; i < vertexCount; i++) {
                    PhysXLib.GetVectorFromArray(usedVertices, physXVertex, i);
                    meshMeshVertices.Add(physXVertex.ToVector());
                }
                meshMesh.SetVertices(meshMeshVertices);
                meshMesh.triangles = usedTris;
                meshMesh.RecalculateNormals();
            }

            base.Setup(attachedRigidBody, vehicleId);
        }
    }

    void OnDrawGizmos() {
        if (meshMesh != null) {
            Vector3 oldScale = transform.localScale;
            transform.localScale = Vector3.one;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1, 0, 1, 0.3f);
            Gizmos.DrawMesh(meshMesh, offset, Quaternion.identity, scale);
            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.identity;
            transform.localScale = oldScale;
        }
    }
}
