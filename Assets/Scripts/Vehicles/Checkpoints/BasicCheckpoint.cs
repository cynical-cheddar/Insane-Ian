using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCheckpoint : MonoBehaviour
{
    public List<Transform> locations;
    public Transform init() {
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
        return locations[0];
    }

    public Transform nextCheckpoint(Transform t) {
        int i = locations.IndexOf(t);
        if (i == -1) {
            Debug.LogError("Location not found");
        }
        if (i == locations.Count - 1) {
            return locations[0];
        } else {
            return locations[i + 1];
        }
    }
}
