using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCheckpoints : MonoBehaviour
{
    public BasicCheckpoint bc;
    private int checkpoints;
    public Transform checkpointT;
    public object activeCheckpoint;
    // Start is called before the first frame update
    void Start()
    {
        checkpointT = bc.init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Checkpoint")) {
            checkpoints++;
            checkpointT = bc.nextCheckpoint(checkpointT);
            Debug.Log(checkpoints);
        }
    }
}
