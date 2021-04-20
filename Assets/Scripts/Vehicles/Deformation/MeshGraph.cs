using System.Collections.Generic;
using UnityEngine;

namespace GraphBending {
    public class MeshGraph {
        public List<VertexGroup> groups {get;}
        public MeshGraph(Mesh mesh, Mesh skeleton, float groupRadius) {
            groups = new List<VertexGroup>();
            List<Vector3> vertices = new List<Vector3>(mesh.vertices);

            for (int i = 0; i < vertices.Count; i++) {
                bool grouped = false;

                //  Check if vertex is within grouping radius of any group
                for (int j = 0; j < groups.Count; j++) {
                    Vector3 distVector = vertices[i] - groups[j].pos;
                    float sqrDistance = distVector.sqrMagnitude;

                    if (sqrDistance <= groupRadius * groupRadius) {
                        //  Add vertex to group
                        groups[j].vertexIndices.Add(i);
                        grouped = true;
                        break;
                    }
                }

                // If no group found for vertex, create new group.
                if (!grouped) {
                    groups.Add(new VertexGroup(i, vertices));
                }
            }

            for (int i = 0; i < mesh.triangles.Length; i += 3) {
                int vertexIndexA = mesh.triangles[i];
                int vertexIndexB = mesh.triangles[i + 1];
                int vertexIndexC = mesh.triangles[i + 2];

                VertexGroup groupA = FindGroupContainingVertex(vertexIndexA);
                VertexGroup groupB = FindGroupContainingVertex(vertexIndexB);
                VertexGroup groupC = FindGroupContainingVertex(vertexIndexC);

                if (groupA != groupB) {
                    if (!groupA.IsAdjacentTo(groupB)) {
                        Edge edge = new Edge(groupA, groupB);
                        groupA.connectingEdges.Add(edge);
                        groupB.connectingEdges.Add(edge);
                    }
                }
                if (groupB != groupC) {
                    if (!groupC.IsAdjacentTo(groupB)) {
                        Edge edge = new Edge(groupC, groupB);
                        groupC.connectingEdges.Add(edge);
                        groupB.connectingEdges.Add(edge);
                    }
                }
                if (groupC != groupA) {
                    if (!groupA.IsAdjacentTo(groupC)) {
                        Edge edge = new Edge(groupA, groupC);
                        groupA.connectingEdges.Add(edge);
                        groupC.connectingEdges.Add(edge);
                    }
                }
            }

            Debug.Log(groups.Count);
            Debug.Log(skeleton.vertices.Length);

            for (int i = 0; i < skeleton.vertices.Length; i++) {
                float minSqrDist = float.MaxValue;
                int targetGroup = -1;
                for (int j = 0; j < groups.Count; j++) {
                    float sqrDist = (skeleton.vertices[i] - groups[j].pos).sqrMagnitude;
                    if (sqrDist < minSqrDist) {
                        targetGroup = j;
                        minSqrDist = sqrDist;
                    }
                }

                Debug.Log(groups[targetGroup].skeletonVertexIndex);
                groups[targetGroup].skeletonVertexIndex = i;
            }
        }

        public VertexGroup FindGroupContainingVertex(int vertexIndex) {
            for (int i = 0; i < groups.Count; i++) {
                if (groups[i].vertexIndices.Contains(vertexIndex)) return groups[i];
            }
            return null;
        }
    }
}