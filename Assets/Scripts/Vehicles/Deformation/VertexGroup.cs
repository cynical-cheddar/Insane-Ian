using System.Collections.Generic;
using UnityEngine;

namespace GraphBending {
    public class VertexGroup {
        public VertexGroup(int vertexIndex, List<Vector3> vertices) {
            vertexIndices = new List<int>();
            connectingEdges = new List<Edge>();
            connectingEdgeLengths = new List<float>();
            vertexIndices.Add(vertexIndex);
            wasMoved = false;
            enqueued = false;
            skeletonVertexIndex = -1;
            UpdatePos(vertices);
        }

        public void UpdatePos(List<Vector3> vertices, bool updateEdgeLengths) {
            pos = vertices[vertexIndices[0]];
            if (updateEdgeLengths) UpdateEdgeLengths();
        }

        public void UpdatePos(List<Vector3> vertices) {
            UpdatePos(vertices, true);
        }

        public void UpdateEdgeLengths() {
            foreach (Edge edge in connectingEdges) {
                edge.UpdateEdgeLength();
            }
        }

        public void MoveTo(List<Vector3> vertices, List<Vector3> skeletonVertices, Vector3 pos) {
            MoveTo(vertices, skeletonVertices, pos, true);
        }

        public void MoveTo(List<Vector3> vertices, List<Vector3> skeletonVertices, Vector3 pos, bool updateEdgeLengths) {
            foreach (int index in vertexIndices) {
                vertices[index] = pos;
            }
            if (skeletonVertexIndex >= 0) {
                skeletonVertices[skeletonVertexIndex] = pos;
            }
            UpdatePos(vertices, updateEdgeLengths);
        }

        public void MoveBy(List<Vector3> vertices, List<Vector3> skeletonVertices, Vector3 shift) {
            MoveBy(vertices, skeletonVertices, shift, true);
        }

        public void MoveBy(List<Vector3> vertices, List<Vector3> skeletonVertices, Vector3 shift, bool updateEdgeLengths) {
            foreach (int index in vertexIndices) {
                vertices[index] += shift;
            }
            if (skeletonVertexIndex >= 0) {
                skeletonVertices[skeletonVertexIndex] += shift;
            }
            UpdatePos(vertices, updateEdgeLengths);
        }

        public bool IsAdjacentTo(VertexGroup vertexGroup) {
            foreach (Edge edge in connectingEdges) {
                if (edge.a == vertexGroup || edge.b == vertexGroup) return true;
            }
            return false;
        }

        public List<int> vertexIndices;
        public int skeletonVertexIndex;

        public List<Edge> connectingEdges;
        public Vector3 pos;
        public bool wasMoved;
        public bool enqueued;
        public List<float> connectingEdgeLengths;
    }
}