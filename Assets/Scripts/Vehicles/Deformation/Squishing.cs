using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squishing : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<VertexGroup> vertexGroups;
    //public GameObject testMarker;
    public List<GameObject> deformationPoints;
    public float vertexWeight = 1;
    public float groupRadius = 0.05f;
    public float explosionRadius = 1f;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = new List<Vector3>(mesh.vertices);

        //  Group similar vertices.
        vertexGroups = VertexGroup.GroupVertices(vertices, groupRadius);

        //Vector3 pos = new Vector3(1.5f, 0, 0);
        //Instantiate<GameObject>(testMarker, pos, Quaternion.identity);

        //ExplodeMeshAt(pos, 1);

        foreach (GameObject deformationPoint in deformationPoints) {
            ExplodeMeshTowardsCentreAt(deformationPoint.transform.position, 1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ExplodeMeshTowardsCentreAt(Vector3 pos, float force) {
        for (int i = 0; i < vertexGroups.Count; i++) {
            Vector3 deformation = vertices[vertexGroups[i].vertices[0]] - pos;
            float deformationForce = (force * (Random.value * 0.2f + 0.9f)) / (1 + deformation.sqrMagnitude);
            Vector3 direction = vertices[vertexGroups[i].vertices[0]] - transform.position;
            direction *= deformationForce / vertexWeight;
            for (int j = 0; j < vertexGroups[i].vertices.Count; j++) {
                if (deformation.magnitude < explosionRadius) {
                    vertices[vertexGroups[i].vertices[j]] -= direction;
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);
    }

    public void ExplodeMeshAt(Vector3 pos, float force) {
        for (int i = 0; i < vertexGroups.Count; i++) {
            Vector3 deformation = vertices[vertexGroups[i].vertices[0]] - pos;
            float deformationForce = (force * (Random.value * 0.2f + 0.9f)) / (1 + deformation.sqrMagnitude);
            deformation.Normalize();
            deformation *= deformationForce / vertexWeight;
            for (int j = 0; j < vertexGroups[i].vertices.Count; j++) {
                vertices[vertexGroups[i].vertices[j]] += deformation;
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);
    }

    public void DeformMeshAt(Vector3 pos, float force) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector3 deformation = vertices[i] - pos;
            deformation.Normalize();
            deformation *= force;
            vertices[i] += deformation;
        }

        mesh.vertices = vertices.ToArray();
        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);
    }

    private class VertexGroup {
        static public List<VertexGroup> GroupVertices(List<Vector3> vertices, float radius) {
            List<VertexGroup> groups = new List<VertexGroup>();

            for (int i = 0; i < vertices.Count; i++) {
                bool grouped = false;

                //  Check if vertex is within grouping radius of any group
                for (int j = 0; j < groups.Count; j++) {
                    float sqrDistance = (vertices[i] - vertices[groups[j].vertices[0]]).sqrMagnitude;

                    if (sqrDistance <= radius * radius) {
                        //  Add vertex to group
                        groups[j].vertices.Add(i);
                        grouped = true;
                        break;
                    }
                }

                // If no group found for vertex, create new group.
                if (!grouped) {
                    groups.Add(new VertexGroup(i));
                }
            }

            return groups;
        }

        public VertexGroup(int vertex) {
            vertices = new List<int>();
            vertices.Add(vertex);
        }

        public List<int> vertices;
    }
}
