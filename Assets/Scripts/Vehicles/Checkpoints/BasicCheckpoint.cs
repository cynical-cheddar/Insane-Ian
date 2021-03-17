using System.Collections.Generic;
using UnityEngine;

public class BasicCheckpoint : MonoBehaviour {
    public List<Transform> locations;
    public int checkpointCount = 50;
    private List<Transform> checkpointList = new List<Transform>();
    private List<int> indexList = new List<int>();
    private int prev1, prev2, next;
    private int current;

    void Start() {
        bool host = true;

        if (host) { //if you are the host, generate checkpoints
            if (locations.Count < 3) {
                Debug.LogError("Less than three checkpoints");
            } else {
                prev1 = Random.Range(0, locations.Count - 1);            
                indexList.Add(prev1); //add one checkpoint
                next = prev1;
                prev2 = prev1;
                for (int i = 0; i < checkpointCount - 1; ++i) { // add sequential checkpoints
                    while (next == prev1 || next == prev2) { //next checkpoint cannot be either of the previous two
                        next = Random.Range(0, locations.Count);
                    }
                    indexList.Add(next);
                    Debug.Log(next);
                    prev2 = prev1;
                    prev1 = next;
                }
            }
        } else { // if not host, get checkpoint index list
            indexList = getListFromHost();
        }
        foreach (int i in indexList){ // turn index list into location list
            checkpointList.Add(locations[i]);
        }

        current = 0;
    }

    private List<int> getListFromHost() {
        throw new System.NotImplementedException();
    }

    public Vector3 nextCheckpoint(Vector3 t) {
        current++;
        if (current >= checkpointList.Count) current = 0;
        return checkpointList[current].position;


    }
}
