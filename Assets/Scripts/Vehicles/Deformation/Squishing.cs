using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBending;
using System.Linq;
using PhysX;
using System.Runtime.CompilerServices;
using Unity.Profiling;

public class Squishing : MonoBehaviour, ICollisionStayEvent, ICollisionEnterEvent
{
    static readonly ProfilerMarker meshDeformationMarker = new ProfilerMarker("MeshDeformation");

    public MeshstateTracker.MeshTypes meshType;
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
    MeshGraph meshGraph;
    public GameObject collisionResolver;
    private PhysXBody resolverBody;

    private PhysXRigidBody myRb;
    public float vertexWeight = 1;
    public float stretchiness = 1000000.1f;
    public float minPenetration = 0.2f;
    public float minDeformationMass = 50;
    //public float maxEdgeLength = 0.6f;
    private List<DeformableMesh> deformableMeshes = null;
    private List<float> oldEdgeSqrLengths = new List<float>();
    private Queue<VertexGroup> vertexQueue = new Queue<VertexGroup>();
    private bool dissipationNeeded = false;

    private Vector3 gizmoSurfaceNormal = Vector3.forward;
    private Vector3 gizmoSurfacePoint;

    private int teamId;

    InterfaceCarDrive4W interfaceCar;
    Mesh originalMesh;

    public void CollisionEnter(){}

    public void CollisionEnter(PhysXCollision other){
        if (other.gameObject.CompareTag("DustGround")) {
            myRb.ghostEnabled = false;
        }
        else {
            myRb.ghostEnabled = true;
        }
    }

    MeshstateTracker meshstateTracker;

    // Start is called before the first frame update.
    void Start() {
        myRb = GetComponent<PhysXRigidBody>();
        
        meshstateTracker = FindObjectOfType<MeshstateTracker>();

        deformableMeshes = new List<DeformableMesh>(GetComponentsInChildren<DeformableMesh>());
        DeformableMesh.Subdivide(deformableMeshes[0].maxEdgeLength, deformableMeshes[0].GetMeshFilter().mesh);
        vertices = new List<Vector3>(deformableMeshes[0].GetMeshFilter().mesh.vertices);

        //  Group similar vertices.
        meshGraph = meshstateTracker.GetMyMeshGraph(meshType);

        originalMesh = Instantiate(deformableMeshes[0].GetMeshFilter().mesh);
        collisionResolver = Instantiate(collisionResolver);
        resolverBody = collisionResolver.GetComponent<PhysXBody>();
        resolverBody.position = new Vector3(0, 10000, 0);

        interfaceCar = GetComponent<InterfaceCarDrive4W>();
        if (interfaceCar!=null) {
            frWheel = interfaceCar.frontRightW;
            frWheelVertexGroup = NearestVertexTo(frWheel.transform.position);
            flWheel = interfaceCar.frontLeftW;
            flWheelVertexGroup = NearestVertexTo(flWheel.transform.position);
            rrWheel = interfaceCar.rearRightW;
            rrWheelVertexGroup = NearestVertexTo(rrWheel.transform.position);
            rlWheel = interfaceCar.rearLeftW;
            rlWheelVertexGroup = NearestVertexTo(rlWheel.transform.position);
        }

        teamId = GetComponent<NetworkPlayerVehicle>().teamId;
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
        for (int i = 1; i < meshGraph.groups.Length; i++) {
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
        closest.SetEnqueued(teamId, true);

        // Move each vertex, making sure that it doesn't stretch too far from its neighbours
        while (vertexQueue.Count > 0) {
            VertexGroup current = vertexQueue.Dequeue();
            current.SetEnqueued(teamId, false);

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

            current.MoveBy(vertices, deformation, false);

            for (int j = 0; j < current.connectingEdges.Count; j++) {
                VertexGroup adjacent = current.connectingEdges[j].OtherVertexGroup(current);

                //  Check if adjacent vertex has been moved.
                if (adjacent.GetWasMoved(teamId)) {
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

            current.SetWasMoved(teamId, true);

            //  Add adjacent, unmoved vertices into the queue for traversal
            for (int j = 0; j < current.connectingEdges.Count; j++) {
                //  Get adjacent vertex group
                VertexGroup adjacent = current.connectingEdges[j].OtherVertexGroup(current);

                //  Add it to the queue if it hasn't already been moved
                if (!adjacent.GetEnqueued(teamId) && !adjacent.GetWasMoved(teamId)) {
                    vertexQueue.Enqueue(adjacent);
                    adjacent.SetEnqueued(teamId, true);
                }
            }
        }

        for (int i = 0; i < meshGraph.groups.Length; i++) {
            meshGraph.groups[i].SetWasMoved(teamId, false);
            if (meshGraph.groups[i].GetEnqueued(teamId)) {
                Debug.LogWarning("Vertex marked as still in queue.");
                meshGraph.groups[i].SetEnqueued(teamId, false);
            }
        }

        //  Update the mesh
        deformableMeshes[0].GetMeshFilter().mesh.SetVertices(vertices);
        deformableMeshes[0].GetMeshFilter().mesh.RecalculateNormals();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBeyondCollisionSurface(Vector3 surfaceNormal, Vector3 surfacePoint, Vector3 vertex) {
        Vector3 relativePosition = vertex - surfacePoint;
        return Vector3.Dot(relativePosition, surfaceNormal) < 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector3 DeformationFromCollisionSurface(Vector3 surfaceNormal, Vector3 surfacePoint, Vector3 vertex) {
        float distBeyondPlane = Vector3.Dot(-surfaceNormal, vertex) - Vector3.Dot(-surfaceNormal, surfacePoint);
        return surfaceNormal * distBeyondPlane;
    }

    public void CollisionStay() {}

    public void CollisionStay(PhysXCollision collision) {

        if (collision.contactCount > 0 && collision.GetContact(0).separation < -minPenetration) {
            if ((collision.rigidBody == null || collision.rigidBody.mass >= minDeformationMass) &&
                !collision.collider.gameObject.CompareTag("DustGround")) {

                meshDeformationMarker.Begin();
            
                bool isInconvenient = collision.collider is PhysXMeshCollider && !((PhysXMeshCollider)collision.collider).convex;

                Vector3 collisionSurfaceNormal = Vector3.zero;
                Vector3 collisionSurfacePoint = Vector3.zero;

                for (int i = 0; i < collision.contactCount; i++) {
                    PhysXContactPoint contactPoint = collision.GetContact(i);
                    float impulseMagnitude = contactPoint.impulse.magnitude;

                    collisionSurfaceNormal += contactPoint.normal;
                    collisionSurfacePoint += contactPoint.point;
                }



                collisionSurfaceNormal /= collision.contactCount;
                collisionSurfacePoint /= collision.contactCount;

                collisionSurfaceNormal = transform.InverseTransformDirection(collisionSurfaceNormal);
                collisionSurfacePoint = transform.InverseTransformPoint(collisionSurfacePoint);

                gizmoSurfaceNormal = collisionSurfaceNormal;
                gizmoSurfacePoint = collisionSurfacePoint;

                Vector3 oldPosition = collision.body.position;
                Quaternion oldRotation = collision.body.rotation;
                collision.body.position = transform.InverseTransformPoint(collision.body.position);
                collision.body.rotation = Quaternion.Inverse(transform.rotation) * collision.body.rotation;

                VertexGroup[] groups = meshGraph.groups;
                for (int i = 0; i < groups.Length; i++) {
                    VertexGroup current = groups[i];
                    Vector3 vertex = current.pos;

                    if (IsBeyondCollisionSurface(collisionSurfaceNormal, collisionSurfacePoint, vertex)) {
                        if (isInconvenient || collision.collider.ClosestPoint(vertex) == vertex) {
                            Vector3 deformation = DeformationFromCollisionSurface(collisionSurfaceNormal, collisionSurfacePoint, vertex);

                            // if (addNoise) deformation *= Random.value * multiplier + addition;

                            current.MoveBy(vertices, deformation, false);
                            current.SetWasMoved(teamId, true);

                            if (!current.GetEnqueued(teamId)) {
                                vertexQueue.Enqueue(current);
                                current.SetEnqueued(teamId, true);
                                // Debug.Log("Vertex group " + current.vertexIndices[0] + " enqueued due to collision");
                            }
                        }
                    }
                }

                collision.body.position = oldPosition;
                collision.body.rotation = oldRotation;

                dissipationNeeded = true;

                meshDeformationMarker.End();
            }
        }
    }

    void Update() {
        // if(curCols >= 0) curCols -= Time.unscaledDeltaTime * 6;
        if (dissipationNeeded) {
            DissipateDeformation(false);
            dissipationNeeded = false;
        }
    }

    public void DissipateDeformation(bool addNoise) {
        foreach (VertexGroup group in vertexQueue) {
            // Debug.Log("Initial queue contains " + group.vertexIndices[0]);
        }

        while (vertexQueue.Count > 0) {
            VertexGroup current = vertexQueue.Dequeue();
            current.SetEnqueued(teamId, false);
            // Debug.Log("Vertex group " + current.vertexIndices[0] + " dequeued");

            oldEdgeSqrLengths.Clear();
            for (int j = 0; j < current.connectingEdges.Count; j++) {
                oldEdgeSqrLengths.Add(current.connectingEdges[j].sqrLength);
            }

            float sqrStretchiness = stretchiness * stretchiness;

            for (int j = 0; j < current.connectingEdges.Count; j++) {
                VertexGroup adjacent = current.connectingEdges[j].OtherVertexGroup(current);

                //  Check if adjacent vertex has been moved.
                if (adjacent.GetWasMoved(teamId)) {
                    //  Get vector of edge between vertices.
                    Vector3 edge = current.pos - adjacent.pos;
                    //  ohno edge too long
                    if (edge.sqrMagnitude > sqrStretchiness * oldEdgeSqrLengths[j]) {
                        //  make edge right length
                        edge.Normalize();
                        float randomNoise = 1; 
                        if (addNoise) randomNoise = Random.value * 0.2f + 0.9f;
                        float edgeStretchiness = stretchiness * randomNoise;
                        edge *= edgeStretchiness * Mathf.Sqrt(oldEdgeSqrLengths[j]);

                        //  move vertices so edge is not too long.
                        current.MoveTo(vertices, adjacent.pos + edge, false);
                        current.connectingEdges[j].UpdateEdgeLength();
                        current.SetWasMoved(teamId, true);
                    }
                }
            }

            if (current.GetWasMoved(teamId)) {
                //  Add adjacent, unmoved vertices into the queue for traversal
                for (int j = 0; j < current.connectingEdges.Count; j++) {
                    //  Get adjacent vertex group
                    VertexGroup adjacent = current.connectingEdges[j].OtherVertexGroup(current);

                    //  Add it to the queue if it hasn't already been moved
                    if (!adjacent.GetEnqueued(teamId) && !adjacent.GetWasMoved(teamId)) {
                        vertexQueue.Enqueue(adjacent);
                        adjacent.SetEnqueued(teamId, true);
                        // Debug.Log("Vertex group " + adjacent.vertexIndices[0] + " enqueued");
                    }
                }
            }

            //moved.Add(current);
            current.SetWasMoved(teamId, true);
        }

        string meshName;
        if (meshType == MeshstateTracker.MeshTypes.interceptor) meshName = "interceptor";
        else if (meshType == MeshstateTracker.MeshTypes.ace) meshName = "ace";
        else if (meshType == MeshstateTracker.MeshTypes.bomber) meshName = "bomber";
        else meshName = "bike";

        for (int i = 0; i < meshGraph.groups.Length; i++) {
            meshGraph.groups[i].SetWasMoved(teamId, false);
            if (meshGraph.groups[i].GetEnqueued(teamId)) {
                Debug.LogWarning("Vertex group " + meshGraph.groups[i].vertexIndices[0] + " marked as still in queue. mesh: " + meshName);
                meshGraph.groups[i].SetEnqueued(teamId, false);
            }
        }

        //  Update the mesh
        deformableMeshes[0].GetMeshFilter().mesh.SetVertices(vertices);
        deformableMeshes[0].GetMeshFilter().mesh.RecalculateNormals();

        if (interfaceCar!=null) {
            frWheel.transform.localPosition = frWheelVertexGroup.pos;
            flWheel.transform.localPosition = flWheelVertexGroup.pos;
            rrWheel.transform.localPosition = rrWheelVertexGroup.pos;
            rlWheel.transform.localPosition = rlWheelVertexGroup.pos;
        }
    }

    public VertexGroup NearestVertexTo(Vector3 point)
    {
        // convert point to local space
        point = transform.InverseTransformPoint(point);

        float minDistanceSqr = Mathf.Infinity;
        VertexGroup nearestVertex = meshGraph.groups[0];

        // scan all vertices to find nearest
        foreach (VertexGroup vertex in meshGraph.groups) {
            Vector3 diff = point - vertex.pos;
            float distSqr = diff.sqrMagnitude;
            if (distSqr < minDistanceSqr) {
                minDistanceSqr = distSqr;
                nearestVertex = vertex;
            }
        }

        return nearestVertex;
    }
}
