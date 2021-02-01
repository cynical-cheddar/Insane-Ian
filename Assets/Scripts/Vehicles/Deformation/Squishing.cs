using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squishing : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    public GameObject testMarker;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = new List<Vector3>(mesh.vertices);

        Instantiate<GameObject>(testMarker, new Vector3(1, 0, 0), Quaternion.identity);

        DeformMeshAt(new Vector3(1, 0, 0), 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeformMeshAt(Vector3 pos, float force) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector3 deformation = vertices[i] - pos;
            deformation.Normalize();
            deformation *= force;
            vertices[i] += deformation;
        }

        mesh.vertices = vertices.ToArray();
        mesh.UploadMeshData(false);
    }
}
