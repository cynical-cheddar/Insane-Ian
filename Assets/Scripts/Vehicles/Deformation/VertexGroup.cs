using System.Collections.Generic;
using UnityEngine;

namespace GraphBending {
    public class VertexGroup {
        public VertexGroup(int vertex) {
            vertexIndices = new List<int>();
            adjacentVertexGroups = new List<VertexGroup>();
            connectingEdgeLengths = new List<float>();
            vertexIndices.Add(vertex);
        }

        public void MoveTo(List<Vector3> vertices, Vector3 pos) {
            foreach (int index in vertexIndices) {
                vertices[index] = pos;
            }
        }

        public void MoveBy(List<Vector3> vertices, Vector3 shift) {
            foreach (int index in vertexIndices) {
                vertices[index] += shift;
            }
        }

        public List<int> vertexIndices;

        public List<VertexGroup> adjacentVertexGroups;
        public List<float> connectingEdgeLengths;
    }
}