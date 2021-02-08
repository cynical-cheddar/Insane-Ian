using System.Collections.Generic;
using UnityEngine;

namespace GraphBending {
    public class Edge {
        public VertexGroup a;
        public VertexGroup b;

        public float sqrLength;
        public float length {
            get {
                return Mathf.Sqrt(sqrLength);
            }

            set {
                sqrLength = value * value;
            }
        }

        public Vector3 edgeVector {
            get {
                return a.pos - b.pos;
            }
        }

        public Edge(VertexGroup a, VertexGroup b) {
            this.a = a;
            this.b = b;
            UpdateEdgeLength();
        }

        public void UpdateEdgeLength() {
            Vector3 edge = a.pos - b.pos;
            sqrLength = edge.sqrMagnitude;
        }

        public VertexGroup OtherVertexGroup(VertexGroup vertexGroup) {
            VertexGroup other = a;
            if (other == vertexGroup) other = b;
            return other;
        }
    }
}