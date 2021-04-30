using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphBending;
using UnityEngine.SceneManagement;
public class MeshstateTracker : MonoBehaviour
{
    // Start is called before the first frame update

    public enum MeshTypes{ interceptor, ace, bomber, bike};


    MeshGraph interceptorMeshGraph;
    MeshGraph aceMeshGraph;

    MeshGraph bomberMeshGraph;

    MeshGraph bikeMeshGraph;

    public float groupRadius = 0.05f;
    public float maxEdgeLength = 0.6f;

    public Mesh interceptorMeshFilter;

    public Mesh aceMeshFilter;
    public Mesh bomberMeshFilter;
    public Mesh bikeMeshFilter;

    void Start(){
        DontDestroyOnLoad(gameObject);
        Invoke(nameof(LateStart), 1f);
    }

    void LateStart()
    {
        DeformableMesh.Subdivide(maxEdgeLength, interceptorMeshFilter);
        interceptorMeshGraph = new MeshGraph(interceptorMeshFilter, groupRadius);
        

        DeformableMesh.Subdivide(maxEdgeLength, aceMeshFilter);
        aceMeshGraph = new MeshGraph(aceMeshFilter, groupRadius);


        DeformableMesh.Subdivide(maxEdgeLength, bomberMeshFilter);
        bomberMeshGraph = new MeshGraph(bomberMeshFilter, groupRadius);


        DeformableMesh.Subdivide(maxEdgeLength, bikeMeshFilter);
        bikeMeshGraph = new MeshGraph(bikeMeshFilter, groupRadius);

        SceneManager.LoadScene("MainMenu");
    }

    public MeshGraph GetMyMeshGraph(MeshTypes meshType){
        if(meshType == MeshTypes.interceptor) return interceptorMeshGraph;
        if(meshType == MeshTypes.ace) return aceMeshGraph;
        if(meshType == MeshTypes.bomber) return bomberMeshGraph;
        if(meshType == MeshTypes.bike) return bikeMeshGraph;
        else{ return interceptorMeshGraph;}
    }
}
