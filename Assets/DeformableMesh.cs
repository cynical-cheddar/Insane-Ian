using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DeformableMesh : MonoBehaviour
{
    private MeshFilter meshFilter = null;
    public float maxEdgeLength = 0.6f;

    void Update() {
        if (!Application.IsPlaying(gameObject)) {
            Subdivide(maxEdgeLength);
        }
    }

    public void Subdivide(float maxEdgeLength) {
        GetMeshFilter();

        Mesh mesh = meshFilter.mesh;
        List<Vector3> vertices = new List<Vector3>(mesh.vertices);
        Dictionary<int, Dictionary<int, int>> splits = new Dictionary<int, Dictionary<int, int>>();

        for (int submesh = 0; submesh < mesh.subMeshCount; submesh++) {
            List<int> tris = new List<int>(mesh.GetTriangles(submesh));

            bool sliced = true;

            while (sliced) {
                sliced = false;
                
                for (int i = 0; i < tris.Count; i += 3) {
                    float lengthA = (vertices[tris[i]] - vertices[tris[i + 1]]).sqrMagnitude;
                    float lengthB = (vertices[tris[i + 1]] - vertices[tris[i + 2]]).sqrMagnitude;
                    float lengthC = (vertices[tris[i + 2]] - vertices[tris[i]]).sqrMagnitude;

                    if (lengthA > maxEdgeLength * maxEdgeLength) {
                        if (lengthA > lengthB) {
                            if (lengthA > lengthC) Slice(i, i + 1, i + 2, vertices, tris, splits);
                            else Slice(i + 2, i, i + 1, vertices, tris, splits);
                        }
                        else {
                            if (lengthB > lengthC) Slice(i + 1, i + 2, i, vertices, tris, splits);
                            else Slice(i + 2, i, i + 1, vertices, tris, splits);
                        }
                        sliced = true;
                    }
                    else if (lengthB > maxEdgeLength * maxEdgeLength) {
                        if (lengthB > lengthC) Slice(i + 1, i + 2, i, vertices, tris, splits);
                        else Slice(i + 2, i, i + 1, vertices, tris, splits);
                        sliced = true;
                    }
                    else if (lengthC > maxEdgeLength * maxEdgeLength) {
                        Slice(i + 2, i, i + 1, vertices, tris, splits);
                        sliced = true;
                    }
                }
            }
            mesh.vertices = vertices.ToArray();
            mesh.SetTriangles(tris.ToArray(), submesh);
        }

        mesh.RecalculateNormals();
    }

    private void Slice(int a, int b, int c, List<Vector3> vertices, List<int> tris, Dictionary<int, Dictionary<int, int>> splits) {
        int ai = tris[a];
        int bi = tris[b];
        int ci = tris[c];

        int lowIndex = Mathf.Min(ai, bi);
        int highIndex = Mathf.Max(ai, bi);

        if (!splits.ContainsKey(lowIndex)) splits.Add(lowIndex, new Dictionary<int, int>());
        if (!splits[lowIndex].ContainsKey(highIndex)) {
            splits[lowIndex].Add(highIndex, vertices.Count);
            vertices.Add(vertices[lowIndex] * 0.5f + vertices[highIndex] * 0.5f);
        }
        int splitIndex = splits[lowIndex][highIndex];

        tris[b] = splitIndex;

        tris.Add(splitIndex);
        tris.Add(bi);
        tris.Add(ci);
    }

    public MeshFilter GetMeshFilter() {
        if (meshFilter == null) {
            meshFilter = GetComponent<MeshFilter>();
        }
        return meshFilter;
    }
}
