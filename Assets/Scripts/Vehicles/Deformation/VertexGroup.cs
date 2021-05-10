using System.Collections.Generic;
using UnityEngine;

namespace GraphBending {
    public class VertexGroup {
        public VertexGroup(int vertexIndex, List<Vector3> vertices) {
            vertexIndices = new List<int>();
            connectingEdges = new List<Edge>();
            connectingEdgeLengths = new List<float>();
            vertexIndices.Add(vertexIndex);
            wasMoved = 0;
            enqueued = 0;
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

        public void MoveTo(List<Vector3> vertices, Vector3 pos) {
            MoveTo(vertices, pos, true);
        }

        public void MoveTo(List<Vector3> vertices, Vector3 pos, bool updateEdgeLengths) {
            foreach (int index in vertexIndices) {
                vertices[index] = pos;
            }
            UpdatePos(vertices, updateEdgeLengths);
        }

        public void MoveBy(List<Vector3> vertices, Vector3 shift) {
            MoveBy(vertices, shift, true);
        }

        public void MoveBy(List<Vector3> vertices, Vector3 shift, bool updateEdgeLengths) {
            foreach (int index in vertexIndices) {
                vertices[index] += shift;
            }
            UpdatePos(vertices, updateEdgeLengths);
        }

        public bool IsAdjacentTo(VertexGroup vertexGroup) {
            foreach (Edge edge in connectingEdges) {
                if (edge.a == vertexGroup || edge.b == vertexGroup) return true;
            }
            return false;
        }

        public void SetEnqueued(int teamId, bool value) {
            if (value) enqueued |= (1 << teamId);
            else enqueued &= ~(1 << teamId);
        }

        public bool GetEnqueued(int teamId) {
            return (enqueued & (1 << teamId)) != 0;
        }

        public void SetWasMoved(int teamId, bool value) {
            if (value) wasMoved |= (1 << teamId);
            else wasMoved &= ~(1 << teamId);
        }

        public bool GetWasMoved(int teamId) {
            return (wasMoved & (1 << teamId)) != 0;
        }

        public List<int> vertexIndices;

        public List<Edge> connectingEdges;
        public Vector3 pos;
        private int wasMoved;
        private int enqueued;
        public List<float> connectingEdgeLengths;
    }
}