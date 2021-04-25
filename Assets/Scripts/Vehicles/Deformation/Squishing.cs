using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBending;
using System.Linq;
using PhysX;

public class Squishing : MonoBehaviour, ICollisionStayEvent
{
    public bool requiresData { get { return true; } }

    PhysXWheelCollider frWheel;
    VertexGroup frWheelVertexGroup;
    PhysXWheelCollider flWheel;

    VertexGroup flWheelVertexGroup;

    PhysXWheelCollider rrWheel;

    VertexGroup rrWheelVertexGroup;

    PhysXWheelCollider rlWheel;

    VertexGroup rlWheelVertexGroup;

    private List<Vector3> vertices;
    private List<Vector3> skeletonVertices = null;
    private MeshGraph meshGraph;
    public GameObject testMarker;
    public GameObject collisionResolver;
    private PhysXBody resolverBody;

    private PhysXRigidBody myRb;
    public float vertexWeight = 1;
    public float groupRadius = 0.05f;
    public float stretchiness = 1000000.1f;
    public float collisionResistance = 200;
    //public float maxEdgeLength = 0.6f;
    private List<DeformableMesh> deformableMeshes = null;
    private List<float> oldEdgeSqrLengths = new List<float>();
    private Queue<VertexGroup> vertexQueue = new Queue<VertexGroup>();

    private Vector3 gizmoSurfaceNormal = Vector3.forward;
    private Vector3 gizmoSurfacePoint;


InterfaceCarDrive4W interfaceCar;
    Mesh originalMesh;

    void OnDrawGizmos() {
        Gizmos.color = new Color(1, 0, 1);
        // Vector3 oldTransormPosition = transform.position;
        // Quaternion oldTransormRotation = transform.rotation;

        // transform.position = gizmoSurfacePoint;
        // transform.rotation = Quaternion.LookRotation(gizmoSurfaceNormal, Vector3.up);

        if (deformableMeshes != null && deformableMeshes.Count > 0) {
            Gizmos.matrix = deformableMeshes[0].transform.localToWorldMatrix;
        }
        // Gizmos.DrawCube(Vector3.zero, new Vector3(1, 1, 0.1f));
        Gizmos.color = Color.white;
        Gizmos.matrix = Matrix4x4.identity;

        // transform.position = oldTransormPosition;
        // transform.rotation = oldTransormRotation;
    }

    // Start is called before the first frame update.
    void Start() {
        myRb = GetComponent<PhysXRigidBody>();
        
        

        deformableMeshes = new List<DeformableMesh>(GetComponentsInChildren<DeformableMesh>());
        deformableMeshes[0].Subdivide(deformableMeshes[0].maxEdgeLength);
        vertices = new List<Vector3>(deformableMeshes[0].GetMeshFilter().mesh.vertices);

        //  Group similar vertices.
        meshGraph = new MeshGraph(deformableMeshes[0].GetMeshFilter().mesh, groupRadius);
        foreach (VertexGroup group in meshGraph.groups) {
            if (group.skeletonVertexIndex >= 0) {
                skeletonVertices[group.skeletonVertexIndex] = group.pos;
            }
        }

        originalMesh = Instantiate(deformableMeshes[0].GetMeshFilter().sharedMesh);
        collisionResolver = Instantiate(collisionResolver);
        resolverBody = collisionResolver.GetComponent<PhysXBody>();
        resolverBody.position = new Vector3(0, 10000, 0);

        if(GetComponent<InterfaceCarDrive4W>()!=null){
        interfaceCar = GetComponent<InterfaceCarDrive4W>();
            if(interfaceCar!=null){
                frWheel = interfaceCar.frontRightW;
                frWheelVertexGroup = NearestVertexTo(frWheel.transform.position);
                flWheel = interfaceCar.frontLeftW;
                flWheelVertexGroup = NearestVertexTo(flWheel.transform.position);
                rrWheel = interfaceCar.rearRightW;
                rrWheelVertexGroup = NearestVertexTo(rrWheel.transform.position);
                rlWheel = interfaceCar.rearLeftW;
                rlWheelVertexGroup = NearestVertexTo(rlWheel.transform.position);
            }
        }
    }

    public void ResetMesh()
    {
        deformableMeshes[0].GetMeshFilter().mesh = Instantiate(originalMesh);
        vertices.Clear();
        vertices.AddRange(originalMesh.vertices);
        foreach (VertexGroup group in meshGraph.groups) {
            group.UpdatePos(vertices, true);
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

        VertexGroup closest = GetClosestVertexGroup(pos);

        //  Make a queue (it breadth first traversal time)
        vertexQueue.Enqueue(closest);
        closest.enqueued = true;

        // Move each vertex, making sure that it doesn't stretch too far from its neighbours
        while (vertexQueue.Count > 0) {
            VertexGroup current = vertexQueue.Dequeue();
            current.enqueued = false;

            oldEdgeSqrLengths.Clear();
            for (int j = 0; j < current.connectingEdges.Count; j++) {
                oldEdgeSqrLengths.Add(current.connectingEdges[j].sqrLength);
            }

            //  Calculate deformation vector
            Vector3 deformation = current.pos - pos;
            float deformationForce = force / deformation.sqrMagnitude;
            if (addNoise) deformationForce *= Random.value * 0.2f + 0.9f;
            deformation.Normalize();
            deformation *= Mathf.Clamp(deformationForce / vertexWeight, 0, 0.5f);

            current.MoveBy(vertices, skeletonVertices, deformation, false);

            for (int j = 0; j < current.connectingEdges.Count; j++) {
                VertexGroup adjacent = current.connectingEdges[j].OtherVertexGroup(current);

                //  Check if adjacent vertex has been moved.
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
                        current.MoveTo(vertices, skeletonVertices, adjacent.pos + edge, false);
                        current.connectingEdges[j].UpdateEdgeLength();
                    }
                }
            }

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
        deformableMeshes[0].GetMeshFilter().mesh.SetVertices(vertices);
        deformableMeshes[0].GetMeshFilter().mesh.RecalculateNormals();
    }

    private bool IsBeyondCollisionSurface(Vector3 surfaceNormal, Vector3 surfacePoint, Vector3 vertex) {
        Vector3 relativePosition = vertex - surfacePoint;
        return Vector3.Dot(relativePosition, surfaceNormal) < 0;
    }

    private Vector3 DeformationFromCollisionSurface(Vector3 surfaceNormal, Vector3 surfacePoint, Vector3 vertex) {
        float distBeyondPlane = Vector3.Dot(-surfaceNormal, vertex) - Vector3.Dot(-surfaceNormal, surfacePoint);
        return surfaceNormal * distBeyondPlane;
    }

    public void CollisionStay() {}

    //  This breaks if this is on a kinematic object (big sad)
    public void CollisionStay(PhysXCollision collision) {
        if ((collision.contactCount > 0 && collision.gameObject.CompareTag("Player")) || (collision.contactCount > 0 && collision.gameObject.CompareTag("DustGround") && myRb.velocity.magnitude > 4)) {
            
            bool isInconvenient = collision.collider is PhysXMeshCollider && !((PhysXMeshCollider)collision.collider).convex;

            Vector3 collisionSurfaceNormal = Vector3.zero;
            Vector3 collisionSurfacePoint = Vector3.zero;
            float sumImpulseMagnitudes = 0;

            for (int i = 0; i < collision.contactCount; i++) {
                PhysXContactPoint contactPoint = collision.GetContact(i);
                float impulseMagnitude = contactPoint.impulse.magnitude;

                collisionSurfaceNormal += contactPoint.normal;
                collisionSurfacePoint += contactPoint.point;


                // collisionSurfaceNormal += contactPoint.normal * impulseMagnitude;
                // collisionSurfacePoint += contactPoint.point * impulseMagnitude;
                // sumImpulseMagnitudes += impulseMagnitude;
            }



            collisionSurfaceNormal /= collision.contactCount;
            collisionSurfacePoint /= collision.contactCount;
            // collisionSurfaceNormal /= sumImpulseMagnitudes;
            // collisionSurfacePoint /= sumImpulseMagnitudes;

            gizmoSurfaceNormal = collisionSurfaceNormal;
            gizmoSurfacePoint = collisionSurfacePoint;
            float multiplier = 0.2f;
            float addition = 0.9f;
            if(collision.collider.gameObject.CompareTag("DustGround")){
                addition = 0f;
                multiplier = 0.05f;
            }
            for (int i = 0; i < meshGraph.groups.Count; i++) {
                    VertexGroup current = meshGraph.groups[i];
                    Vector3 vertex = transform.TransformPoint(vertices[current.vertexIndices[0]]);

                    if (IsBeyondCollisionSurface(collisionSurfaceNormal, collisionSurfacePoint, vertex)) {
                        if (isInconvenient || collision.collider.ClosestPoint(vertex) == vertex) {
                            Vector3 deformation = DeformationFromCollisionSurface(collisionSurfaceNormal, collisionSurfacePoint, vertex);
                            deformation = transform.InverseTransformDirection(deformation);
                            // Debug.Log(deformation);

                            //if (addNoise) deformation *= Random.value * 0.2f + 0.9f;
                            deformation *= Random.value * multiplier +addition;

                            current.MoveBy(vertices, skeletonVertices, deformation, false);
                            current.wasMoved = true;
                            //moved.Add(current);
                            vertexQueue.Enqueue(current);
                            current.enqueued = true;
                        }
                    }
                }

                DissipateDeformation(true);
                if(interfaceCar!=null){
                    frWheel.transform.localPosition = frWheelVertexGroup.pos;
                    flWheel.transform.localPosition = flWheelVertexGroup.pos;
                    rrWheel.transform.localPosition = rrWheelVertexGroup.pos;
                    rlWheel.transform.localPosition = rlWheelVertexGroup.pos;
                }
            

            // Vector3 collisionNormal = collision.GetContact(0).normal;
            // Vector3 collisionForce = collision.impulse;
            // if (Vector3.Dot(collisionForce, collisionNormal) < 0) collisionForce = -collisionForce;
            // collisionForce /= Time.fixedDeltaTime;
            // collisionForce /= collisionResistance;
            // collisionForce = transform.InverseTransformDirection(collisionForce);

            // if (collisionForce.sqrMagnitude >= 0.01f) {
            //     if (collision.collider is PhysXMeshCollider && !((PhysXMeshCollider)collision.collider).convex) {
            //         resolverBody.position = collision.GetContact(0).point;
            //         resolverBody.rotation = Quaternion.LookRotation(-collision.GetContact(0).normal);
            //         PhysXCollider collider = collisionResolver.GetComponent<PhysXBoxCollider>();
            //         CollideMesh(collider, collisionForce, true);
            //         resolverBody.position = new Vector3(0, 10000, 0);
            //     }
            //     else {
            //         CollideMesh(collision.collider, collisionForce, true);
            //     }
            // }
        }
    }

    public void DissipateDeformation(bool addNoise) {
        while (vertexQueue.Count > 0) {
            VertexGroup current = vertexQueue.Dequeue();
            current.enqueued = false;

            oldEdgeSqrLengths.Clear();
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
                        current.MoveTo(vertices, skeletonVertices, adjacent.pos + edge, false);
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
        deformableMeshes[0].GetMeshFilter().mesh.SetVertices(vertices);
        deformableMeshes[0].GetMeshFilter().mesh.RecalculateNormals();
    }

    public void CollideMesh(PhysXCollider collider, Vector3 collisionForce, bool addNoise) {
        

        //List<VertexGroup> moved = new List<VertexGroup>();

        //  Make a queue (it breadth first traversal time)

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

                current.MoveBy(vertices, skeletonVertices, deformation, false);
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

            oldEdgeSqrLengths.Clear();
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
                        current.MoveTo(vertices, skeletonVertices, adjacent.pos + edge, false);
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
        deformableMeshes[0].GetMeshFilter().mesh.SetVertices(vertices);
        deformableMeshes[0].GetMeshFilter().mesh.RecalculateNormals();
        //meshCollider.sharedMesh = mesh;

    }


    public VertexGroup NearestVertexTo(Vector3 point)
    {
        // convert point to local space
        point = transform.InverseTransformPoint(point);



        float minDistanceSqr = Mathf.Infinity;
        VertexGroup nearestVertex = meshGraph.groups[0];
        // scan all vertices to find nearest
        foreach (VertexGroup vertex in meshGraph.groups)
        {
            Vector3 diff = point-vertex.pos;
            float distSqr = diff.sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                nearestVertex = vertex;
            }
        }

        return nearestVertex;


    }


}
