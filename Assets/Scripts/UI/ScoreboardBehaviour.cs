using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardBehaviour : MonoBehaviour
{

    GamestateTracker gamestateTracker;
    [SerializeField] GamestateTracker.PlayerSchema playerSchema;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        // Wait for the host to finish loading first
        Invoke("loadPlayerSchema", 0.1f);
    }

    public void loadPlayerSchema() {
        playerSchema = gamestateTracker.schema;
    }

    // Update is called once per frame
    void Update() {
        
    }

}
