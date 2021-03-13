using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBending;
using System.Linq;

public class Squishing : MonoBehaviour
{
    private List<Vector3> vertices;
    private MeshGraph meshGraph;
    public GameObject testMarker;
    public GameObject collisionResolver;
    public float vertexWeight = 1;
    public float groupRadius = 0.05f;
    public float stretchiness = 1000000.1f;
    public float collisionResistance = 200;
    //public float maxEdgeLength = 0.6f;
    private List<DeformableMesh> deformableMeshes;

    Mesh originalMesh;

    // Start is called before the first frame update.
    void Start() {
        deformableMeshes = new List<DeformableMesh>(GetComponentsInChildren<DeformableMesh>());
        //deformableMeshes[0].Subdivide(maxEdgeLength);
        vertices = new List<Vector3>(deformableMeshes[0].GetMeshFilter().mesh.vertices);

        //  Group similar vertices.
        meshGraph = new MeshGraph(deformableMeshes[0].GetMeshFilter().mesh, groupRadius);

        originalMesh = Instantiate(deformableMeshes[0].GetMeshFilter().sharedMesh);
        collisionResolver = Instantiate(collisionResolver);
    }

    public void ResetMesh() {
        deformableMeshes[0].GetMeshFilter().mesh = Instantiate(originalMesh);
        vertices = originalMesh.vertices.ToList();
        foreach (VertexGroup group in meshGraph.groups) {
            group.UpdatePos(deformableMeshes[0].GetMeshFilter().mesh.vertices.ToList(), true);
        }

    }

    // Return the closest vertex group to the given postion
    VertexGroup GetClosestVertexGroup(Vector3 pos) {
        VertexGroup closest = meshGraph.groups[0];
        float closestDist = (vertices[closest.vertexIndices[0]] - pos).sqrMagnitude;
        for (int i = 1; i < meshGraph.groups.Count; i++) {
            float newDist = (vertices[meshGraph.groups[i].vertexIndices[0]] - pos).sqrMagnitude;
            if (newDist < closestDist) {
                closestDist = newDist;
                closest = meshGraph.groups[i];
            }
        }
        return closest;
    }

    // Explode the mesh at a given position, with a given force
    public void ExplodeMeshAt(Vector3 pos, float force, bool addNoise = true) {
        pos = transform.InverseTransformPoint(pos);

        List<VertexGroup> moved = new List<VertexGroup>();

        VertexGroup closest = GetClosestVertexGroup(pos);

        //  Make a queue (it breadth first traversal time)
        Queue<VertexGroup> vertexQueue = new Queue<VertexGroup>();
        vertexQueue.Enqueue(closest);

        // Move each vertex, making sure that it doesn't stretch too far from its neighbours
        while (vertexQueue.Count > 0) {
            VertexGroup current = vertexQueue.Dequeue();

            List<float> oldEdgeSqrLengths = new List<float>();
            for (int j = 0; j < current.connectingEdges.Count; j++) {
                oldEdgeSqrLengths.Add(current.connectingEdges[j].sqrLength);
            }

            //  Calculate deformation vector
            Vector3 deformation = current.pos - pos;
            float deformationForce = force / deformation.sqrMagnitude;
            if (addNoise) deformationForce *= Random.value * 0.2f + 0.9f;
            deformation.Normalize();
            deformation *= Mathf.Clamp(deformationForce / vertexWeight, 0, 0.5f);

            current.MoveBy(vertices, deformation, false);

            for (int j = 0; j < current.connectingEdges.Count; j++) {
                VertexGroup adjacent = current.connectingEdges[j].OtherVertexGroup(current);

                //  Check if adjacent vertex has been moved.
                if (moved.Contains(adjacent)) {
                    //  Get vector of edge between vertices.
                    Vector3 edge = current.pos - adjacent.pos;
                    //  ohno edge too long
                    if (edge.sqrMagnitude > stretchiness * stretchiness * oldEdgeSqrLengths[j]) {
                        //  make edge right length
                        edge.Normalize();
                        float randomNoise = 1; 
                        if (addNoise) randomNoise = Random.value * 0.2f + 0.9f;
                        float edgeStretchiness = stretchiness * randomNoise;
                        edge *= edgeStretchiness * Mathf.Sqrt(oldEdgeSqrLengths[j]);

                        //  move vertices so edge is not too long.
                        current.MoveTo(vertices, adjacent.pos + edge, false);
                        current.connectingEdges[j].UpdateEdgeLength();
                    }
                }
            }

            moved.Add(current);

            //  Add adjacent, unmoved vertices into the queue for traversal
            for (int j = 0; j < current.connectingEdges.Count; j++) {
                //  Get adjacent vertex group
                VertexGroup adjacent = current.connectingEdges[j].OtherVertexGroup(current);

                //  Add it to the queue if it hasn't already been moved
                if (!vertexQueue.Contains(adjacent) && !moved.Contains(adjacent)) {
                    vertexQueue.Enqueue(adjacent);
                }
            }
        }

        //  Update the mesh
        deformableMeshes[0].GetMeshFilter().mesh.vertices = vertices.ToArray();
        deformableMeshes[0].GetMeshFilter().mesh.RecalculateNormals();
    }

    //  This breaks if this is on a kinematic object (big sad)
    void OnCollisionEnter(Collision collision) {
        Vector3 collisionNormal = collision.GetContact(0).normal;
        Vector3 collisionForce = collision.impulse;
        if (Vector3.Dot(collisionForce, collisionNormal) < 0) collisionForce = -collisionForce;
        collisionForce /= Time.fixedDeltaTime;
        collisionForce /= collisionResistance;
        collisionForce = transform.InverseTransformDirection(collisionForce);

        if (collisionForce.sqrMagnitude >= 0.01f) {
            if (collision.collider is TerrainCollider || (collision.collider is MeshCollider && !((MeshCollider)collision.collider).convex)) {
                collisionResolver.transform.position = collision.GetContact(0).point;
                collisionResolver.transform.rotation = Quaternion.LookRotation(-collision.GetContact(0).normal);
                Collider collider = collisionResolver.GetComponent<BoxCollider>();
                CollideMesh(collider, collisionForce, true);
                collisionResolver.transform.position = new Vector3(0, 10000, 0);
            }
            else {
                CollideMesh(collision.collider, collisionForce, true);
            }
        }
    }

    public void CollideMesh(Collider collider, Vector3 collisionForce, bool addNoise) {
        

        //List<VertexGroup> moved = new List<VertexGroup>();

        //  Make a queue (it breadth first traversal time)
        Queue<VertexGroup> vertexQueue = new Queue<VertexGroup>();

        for (int i = 0; i < meshGraph.groups.Count; i++) {
            VertexGroup current = meshGraph.groups[i];
            Vector3 vertex = transform.TransformPoint(vertices[current.vertexIndices[0]]);

            if (collider.ClosestPoint(vertex) == vertex) {
                Vector3 deformation = collisionForce;
                deformation /= vertexWeight;

                if (deformation.sqrMagnitude > 0.25f) {
                    deformation.Normalize();
                    deformation *= 0.5f;
                }
                if (addNoise) deformation *= Random.value * 0.2f + 0.9f;

                current.MoveBy(vertices, deformation, false);
                current.wasMoved = true;
                //moved.Add(current);
                vertexQueue.Enqueue(current);
                current.enqueued = true;
            }
        }

        // Move each vertex, making sure that it doesn't stretch too far from its neighbours
        while (vertexQueue.Count > 0) {
            VertexGroup current = vertexQueue.Dequeue();
            current.enqueued = false;

            List<float> oldEdgeSqrLengths = new List<float>();
            for (int j = 0; j < current.connectingEdges.Count; j++) {
                oldEdgeSqrLengths.Add(current.connectingEdges[j].sqrLength);
            }

            for (int j = 0; j < current.connectingEdges.Count; j++) {
                VertexGroup adjacent = current.connectingEdges[j].OtherVertexGroup(current);

                //  Check if adjacent vertex has been moved.
                //if (moved.Contains(adjacent)) {
                if (adjacent.wasMoved) {
                    //  Get vector of edge between vertices.
                    Vector3 edge = current.pos - adjacent.pos;
                    //  ohno edge too long
                    if (edge.sqrMagnitude > stretchiness * stretchiness * oldEdgeSqrLengths[j]) {
                        //  make edge right length
                        edge.Normalize();
                        float randomNoise = 1; 
                        if (addNoise) randomNoise = Random.value * 0.2f + 0.9f;
                        float edgeStretchiness = stretchiness * randomNoise;
                        edge *= edgeStretchiness * Mathf.Sqrt(oldEdgeSqrLengths[j]);

                        //  move vertices so edge is not too long.
                        current.MoveTo(vertices, adjacent.pos + edge, false);
                        current.connectingEdges[j].UpdateEdgeLength();
                    }
                }
            }

            //moved.Add(current);
            current.wasMoved = true;

            //  Add adjacent, unmoved vertices into the queue for traversal
            for (int j = 0; j < current.connectingEdges.Count; j++) {
                //  Get adjacent vertex group
                VertexGroup adjacent = current.connectingEdges[j].OtherVertexGroup(current);

                //  Add it to the queue if it hasn't already been moved
                if (!adjacent.enqueued && !adjacent.wasMoved) {
                    vertexQueue.Enqueue(adjacent);
                    adjacent.enqueued = true;
                }
            }
        }

        for (int i = 0; i < meshGraph.groups.Count; i++) {
            meshGraph.groups[i].wasMoved = false;
            if (meshGraph.groups[i].enqueued) {
                Debug.LogWarning("Vertex marked as still in queue.");
                meshGraph.groups[i].enqueued = false;
            }
        }

        //  Update the mesh
        deformableMeshes[0].GetMeshFilter().mesh.vertices = vertices.ToArray();
        deformableMeshes[0].GetMeshFilter().mesh.RecalculateNormals();

        //meshCollider.sharedMesh = mesh;

    }
}
