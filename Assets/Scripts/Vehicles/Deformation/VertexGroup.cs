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

        public List<int> vertexIndices;

        public List<VertexGroup> adjacentVertexGroups;
        public List<float> connectingEdgeLengths;
    }
}