using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCheckpoints : MonoBehaviour
{
    private int checkpoints = 0;
    private BasicCheckpoint bc;
    public Vector3 checkpointPos;
    // Start is called before the first frame update
    void Start()
    {
        bc = (BasicCheckpoint)FindObjectOfType(typeof(BasicCheckpoint));
        if (!bc) Debug.LogWarning("No BasicCheckpoint object could be found (this is fine in menus but BAD in game");
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Checkpoint")) {
            checkpoints++;
            checkpointPos = bc.NextCheckpoint(checkpointPos);
            Debug.Log(checkpoints);
            bc.gameObject.transform.position = checkpointPos;
        }
    }
}
