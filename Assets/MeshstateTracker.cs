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

        System.GC.Collect();
        Debug.Log("IanMeshDone");

        //DeformableMesh.Subdivide(maxEdgeLength, aceMeshFilter);
        aceMeshGraph = new MeshGraph(aceMeshFilter, groupRadius);

        System.GC.Collect();
        Debug.Log("AceMeshDone");

        DeformableMesh.Subdivide(maxEdgeLength, bomberMeshFilter);
        bomberMeshGraph = new MeshGraph(bomberMeshFilter, groupRadius);

        System.GC.Collect();
        Debug.Log("BomberMeshDone");

        //DeformableMesh.Subdivide(maxEdgeLength, bikeMeshFilter);
        bikeMeshGraph = new MeshGraph(bikeMeshFilter, groupRadius);

        System.GC.Collect();
        Debug.Log("BikeMeshDone");

        SceneManager.LoadScene("MainMenu");

        //StartCoroutine(nameof(LoadMeshes));
    }

    IEnumerator LoadMeshes() {
        DeformableMesh.Subdivide(maxEdgeLength, interceptorMeshFilter);
        interceptorMeshGraph = new MeshGraph(interceptorMeshFilter, groupRadius);

        System.GC.Collect();

        yield return new WaitForSecondsRealtime(5f);

        DeformableMesh.Subdivide(maxEdgeLength, aceMeshFilter);
        aceMeshGraph = new MeshGraph(aceMeshFilter, groupRadius);

        System.GC.Collect();

        yield return new WaitForSecondsRealtime(5f);

        DeformableMesh.Subdivide(maxEdgeLength, bomberMeshFilter);
        bomberMeshGraph = new MeshGraph(bomberMeshFilter, groupRadius);

        System.GC.Collect();

        yield return new WaitForSecondsRealtime(5f);

        DeformableMesh.Subdivide(maxEdgeLength, bikeMeshFilter);
        bikeMeshGraph = new MeshGraph(bikeMeshFilter, groupRadius);

        System.GC.Collect();

        yield return new WaitForSecondsRealtime(5f);

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
