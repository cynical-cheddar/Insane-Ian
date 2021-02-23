using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformableMesh : MonoBehaviour
{
    private MeshFilter meshFilter = null;

    public MeshFilter GetMeshFilter() {
        if (meshFilter == null) {
            meshFilter = GetComponent<MeshFilter>();
        }
        return meshFilter;
    }
}
