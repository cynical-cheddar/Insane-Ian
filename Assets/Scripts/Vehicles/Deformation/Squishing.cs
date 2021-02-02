using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBending;

public class Squishing : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<VertexGroup> vertexGroups;
    //public GameObject testMarker;
    public List<GameObject> deformationPoints;
    public float vertexWeight = 1;
    public float groupRadius = 0.05f;
    public float stretchiness = 1.2f;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = new List<Vector3>(mesh.vertices);

        //  Group similar vertices.
        MeshGraph meshGraph = new MeshGraph(mesh, groupRadius);
        vertexGroups = meshGraph.groups;

        //Vector3 pos = new Vector3(1.5f, 0, 0);
        //Instantiate<GameObject>(testMarker, pos, Quaternion.identity);

        //ExplodeMeshAt(pos, 1);

        foreach (GameObject deformationPoint in deformationPoints) {
            //ExplodeMeshTowardsCentreAt(deformationPoint.transform.position, 1f);
            ExplodeMeshAt(deformationPoint.transform.position, 1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ExplodeMeshAtStretchClamped(Vector3 pos, float force) {

    }

    public void ExplodeMeshAt(Vector3 pos, float force) {
        for (int i = 0; i < vertexGroups.Count; i++) {
            Vector3 deformation = vertices[vertexGroups[i].vertexIndices[0]] - pos;
            float deformationForce = (force * (Random.value * 0.2f + 0.9f)) / (deformation.sqrMagnitude);
            deformation.Normalize();
            deformation *= Mathf.Clamp(deformationForce / vertexWeight, 0, 0.5f);
            for (int j = 0; j < vertexGroups[i].vertexIndices.Count; j++) {
                    vertices[vertexGroups[i].vertexIndices[j]] += deformation;
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
}
