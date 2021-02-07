using System.Collections.Generic;
using UnityEngine;

namespace GraphBending {
    public class VertexGroup {
        public VertexGroup(int vertex) {
            vertexIndices = new List<int>();
            connectingEdges = new List<Edge>();
            connectingEdgeLengths = new List<float>();
            vertexIndices.Add(vertex);
        }

        public void UpdatePos(List<Vector3> vertices) {
            pos = vertices[vertexIndices[0]];
            UpdateEdgeLengths();
        }

        public void UpdateEdgeLengths() {
            foreach (Edge edge in connectingEdges) {
                edge.UpdateEdgeLength();
            }
        }

        public void MoveTo(List<Vector3> vertices, Vector3 pos) {
            foreach (int index in vertexIndices) {
                vertices[index] = pos;
            }
            UpdatePos(vertices);
        }

        public void MoveBy(List<Vector3> vertices, Vector3 shift) {
            foreach (int index in vertexIndices) {
                vertices[index] += shift;
            }
            UpdatePos(vertices);
        }

        public bool IsAdjacentTo(VertexGroup vertexGroup) {
            foreach (Edge edge in connectingEdges) {
                if (edge.a == vertexGroup || edge.b == vertexGroup) return true;
            }
            return false;
        }

        public List<int> vertexIndices;

        public List<Edge> connectingEdges;
        public Vector3 pos;
        public List<float> connectingEdgeLengths;
    }
}