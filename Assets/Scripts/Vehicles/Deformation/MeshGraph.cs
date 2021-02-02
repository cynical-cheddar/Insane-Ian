using System.Collections.Generic;
using UnityEngine;

namespace GraphBending {
    public class MeshGraph {
        public List<VertexGroup> groups {get;}
        public MeshGraph(Mesh mesh, float groupRadius) {
            groups = new List<VertexGroup>();
            List<Vector3> vertices = new List<Vector3>(mesh.vertices);

            for (int i = 0; i < vertices.Count; i++) {
                bool grouped = false;

                //  Check if vertex is within grouping radius of any group
                for (int j = 0; j < groups.Count; j++) {
                    Vector3 distVector = vertices[i] - vertices[groups[j].vertexIndices[0]];
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
                    groups.Add(new VertexGroup(i));
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
                    if (!groupA.adjacentVertexGroups.Contains(groupB)) {
                        groupA.adjacentVertexGroups.Add(groupB);
                        groupA.connectingEdgeLengths.Add((vertices[groupA.vertexIndices[0]] - vertices[groupB.vertexIndices[0]]).magnitude);
                    }
                    if (!groupB.adjacentVertexGroups.Contains(groupA)) {
                        groupB.adjacentVertexGroups.Add(groupA);
                        groupB.connectingEdgeLengths.Add((vertices[groupB.vertexIndices[0]] - vertices[groupA.vertexIndices[0]]).magnitude);
                    }
                }
                if (groupA != groupC) {
                    if (!groupA.adjacentVertexGroups.Contains(groupC)) {
                        groupA.adjacentVertexGroups.Add(groupC);
                        groupA.connectingEdgeLengths.Add((vertices[groupA.vertexIndices[0]] - vertices[groupC.vertexIndices[0]]).magnitude);
                    }
                    if (!groupC.adjacentVertexGroups.Contains(groupA)) {
                        groupC.adjacentVertexGroups.Add(groupA);
                        groupC.connectingEdgeLengths.Add((vertices[groupC.vertexIndices[0]] - vertices[groupA.vertexIndices[0]]).magnitude);
                    }
                }
                if (groupC != groupB) {
                    if (!groupC.adjacentVertexGroups.Contains(groupB)) {
                        groupC.adjacentVertexGroups.Add(groupB);
                        groupC.connectingEdgeLengths.Add((vertices[groupC.vertexIndices[0]] - vertices[groupB.vertexIndices[0]]).magnitude);
                    }
                    if (!groupB.adjacentVertexGroups.Contains(groupC)) {
                        groupB.adjacentVertexGroups.Add(groupC);
                        groupB.connectingEdgeLengths.Add((vertices[groupB.vertexIndices[0]] - vertices[groupC.vertexIndices[0]]).magnitude);
                    }
                }
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