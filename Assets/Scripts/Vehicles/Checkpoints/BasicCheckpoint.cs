using System.Collections.Generic;
using UnityEngine;

public class BasicCheckpoint : MonoBehaviour {
    public List<Transform> locations;
    public int checkpointCount = 50;
    private List<Transform> checkpointList = new List<Transform>();
    private int prev, next;
    private int current;

    void Start() {
        if (locations.Count < 2) {
            Debug.LogError("Less than two checkpoints");
        } else {
            prev = Random.Range(0, locations.Count - 1);            
            checkpointList.Add(locations[prev]);
            next = prev;
            for (int i = 0; i < checkpointCount - 1; ++i) {
                while (next == prev) {
                    next = Random.Range(0, locations.Count);
                }
                checkpointList.Add(locations[next]);
                Debug.Log(locations[next]);
                prev = next;
            }
        }
        current = 0;
    }

    public Vector3 nextCheckpoint(Vector3 t) {
        current++;
        if (current >= checkpointList.Count) current = 0;
        return checkpointList[current].position;


    }
}
