using System.Collections.Generic;
using UnityEngine;

public class BasicCheckpoint : MonoBehaviour {
    public List<Transform> locations;
    public Vector3 init() {
        if (locations.Count < 1) {
            Debug.LogError("Less than one checkpoint");
        } else {
            int count = locations.Count;
            int last = count - 1;
            for (int i = 0; i < last; ++i) {
                int r = Random.Range(i, count);
                Transform tmp = locations[i];
                locations[i] = locations[r];
                locations[r] = tmp;
            }
        }
        return locations[0].position;
    }

    public Vector3 nextCheckpoint(Vector3 t) {
        int count = 0;
        int i = -1;
        List<Transform> newlist = new List<Transform>();

        //find the index of the current position
        foreach (Transform transform in locations) {
            if (transform.position == t) i = count;
            else newlist.Add(transform);
            count += 1;
        }

        //if not found panic, return new random position
        if (i == -1) {
            Debug.LogError("Location not found");
        }
        return newlist[Random.Range(0, count - 2)].position;


    }
}
