using System.Collections.Generic;
using UnityEngine;

namespace GraphBending {
    public class MeshGraph {
        public VertexGroup[] groups {get;}
        public MeshGraph(Mesh mesh, float groupRadius) {
            List<VertexGroup> groupList = new List<VertexGroup>();
            List<Vector3> vertices = new List<Vector3>(mesh.vertices);

            for (int i = 0; i < vertices.Count; i++) {
                bool grouped = false;

                //  Check if vertex is within grouping radius of any group
                for (int j = 0; j < groupList.Count; j++) {
                    Vector3 distVector = vertices[i] - groupList[j].pos;
                    float sqrDistance = distVector.sqrMagnitude;

                    if (sqrDistance <= groupRadius * groupRadius) {
                        //  Add vertex to group
                        groupList[j].vertexIndices.Add(i);
                        grouped = true;
                        break;
                    }
                }

                // If no group found for vertex, create new group.
                if (!grouped) {
                    groupList.Add(new VertexGroup(i, vertices));
                }
            }

            for (int i = 0; i < mesh.triangles.Length; i += 3) {
                int vertexIndexA = mesh.triangles[i];
                int vertexIndexB = mesh.triangles[i + 1];
                int vertexIndexC = mesh.triangles[i + 2];

                VertexGroup groupA = FindGroupContainingVertex(groupList, vertexIndexA);
                VertexGroup groupB = FindGroupContainingVertex(groupList, vertexIndexB);
                VertexGroup groupC = FindGroupContainingVertex(groupList, vertexIndexC);

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

            groups = groupList.ToArray();
        }

        private VertexGroup FindGroupContainingVertex(List<VertexGroup> groupList, int vertexIndex) {
            for (int i = 0; i < groupList.Count; i++) {
                if (groupList[i].vertexIndices.Contains(vertexIndex)) return groupList[i];
            }
            return null;
        }
    }
}