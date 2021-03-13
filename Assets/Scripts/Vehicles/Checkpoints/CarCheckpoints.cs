using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCheckpoints : MonoBehaviour
{
    private int checkpoints = 0;
    private BasicCheckpoint bc;
    public Vector3 checkpointPos;
    private object activeCheckpoint;
    private Vector3 prevT;
    // Start is called before the first frame update
    void Start()
    {
        bc = (BasicCheckpoint)FindObjectOfType(typeof(BasicCheckpoint));
        if (bc) Debug.Log("BasicCheckpoint object found: " + bc.name);
        else Debug.Log("No BasicCheckpoint object could be found");
        checkpointPos = bc.init();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Checkpoint")) {
            checkpoints++;
            checkpointPos = bc.nextCheckpoint(checkpointPos);
            Debug.Log(checkpoints);
            bc.gameObject.transform.position = checkpointPos;
        }
    }
}
