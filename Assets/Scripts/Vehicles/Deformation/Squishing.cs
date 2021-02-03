using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBending;

public class Squishing : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    MeshGraph meshGraph;
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
        meshGraph = new MeshGraph(mesh, groupRadius);

        //Vector3 pos = new Vector3(1.5f, 0, 0);
        //Instantiate<GameObject>(testMarker, pos, Quaternion.identity);

        //ExplodeMeshAt(pos, 1);

        foreach (GameObject deformationPoint in deformationPoints) {
            //ExplodeMeshTowardsCentreAt(deformationPoint.transform.position, 1f);
            ExplodeMeshAtStretchClamped(deformationPoint.transform.position, 1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ExplodeMeshAtStretchClamped(Vector3 pos, float force) {
        List<VertexGroup> moved = new List<VertexGroup>();

        VertexGroup closest = meshGraph.groups[0];
        float closestDist = (vertices[closest.vertexIndices[0]] - pos).sqrMagnitude;
        for (int i = 1; i < meshGraph.groups.Count; i++) {
            float newDist = (vertices[meshGraph.groups[i].vertexIndices[0]] - pos).sqrMagnitude;
            if (newDist < closestDist) {
                closestDist = newDist;
                closest = meshGraph.groups[i];
            }
        }

        Queue<VertexGroup> vertexQueue = new Queue<VertexGroup>();
        vertexQueue.Enqueue(closest);
        moved.Add(closest);

        int noplsstop = 0;

        while (vertexQueue.Count > 0 && noplsstop < 5000) {
            VertexGroup current = vertexQueue.Dequeue();

            Vector3 deformation = vertices[current.vertexIndices[0]] - pos;
            float deformationForce = force / deformation.sqrMagnitude;
            //deformationForce *= Random.value * 0.2f + 0.9f;
            deformation.Normalize();
            deformation *= Mathf.Clamp(deformationForce / vertexWeight, 0, 0.5f);
            for (int j = 0; j < current.vertexIndices.Count; j++) {
                vertices[current.vertexIndices[j]] += deformation;
            }
            for (int j = 0; j < current.adjacentVertexGroups.Count; j++) {
                //  Check if adjacent vertex has been moved.
                if (moved.Contains(current.adjacentVertexGroups[j])) {
                    //  Get position of adjacent moved vertex.
                    Vector3 adjacentVertex = vertices[current.adjacentVertexGroups[j].vertexIndices[0]];
                    //  Get vector of edge between vertices.
                    Vector3 edge = vertices[current.vertexIndices[0]] - adjacentVertex;
                    //  ohno edge too long
                    if (edge.sqrMagnitude > stretchiness * stretchiness * current.connectingEdgeLengths[j] * current.connectingEdgeLengths[j]) {
                        //  make edge right length
                        edge.Normalize();
                        edge *= stretchiness * current.connectingEdgeLengths[j];

                        //  move vertices so edge is not too long.
                        for (int k = 0; k < current.vertexIndices.Count; k++) {
                            vertices[current.vertexIndices[k]] = adjacentVertex + edge;
                        }

                        current.connectingEdgeLengths[j] = edge.magnitude;

                        for (int k = 0; k < current.vertexIndices.Count; k++) {
                            //  Find the index of the current vertex in the adjacent vertex's adjacent vertices
                            if (current.adjacentVertexGroups[j].adjacentVertexGroups[k] == current) {
                                current.adjacentVertexGroups[j].connectingEdgeLengths[k] = edge.magnitude;
                                break;
                            }
                        }
                    }
                }
            }

            for (int j = 0; j < current.adjacentVertexGroups.Count; j++) {
                if (!moved.Contains(current.adjacentVertexGroups[j])) {
                    vertexQueue.Enqueue(current.adjacentVertexGroups[j]);
                    moved.Add(current.adjacentVertexGroups[j]);
                }
            }

            noplsstop++;
        }

        mesh.vertices = vertices.ToArray();
        mesh.RecalculateNormals();
    }

    public void ExplodeMeshAt(Vector3 pos, float force) {
        for (int i = 0; i < meshGraph.groups.Count; i++) {
            Vector3 deformation = vertices[meshGraph.groups[i].vertexIndices[0]] - pos;
            float deformationForce = (force * (Random.value * 0.2f + 0.9f)) / (deformation.sqrMagnitude);
            deformation.Normalize();
            deformation *= Mathf.Clamp(deformationForce / vertexWeight, 0, 0.5f);
            for (int j = 0; j < meshGraph.groups[i].vertexIndices.Count; j++) {
                    vertices[meshGraph.groups[i].vertexIndices[j]] += deformation;
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
