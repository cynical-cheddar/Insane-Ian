using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamestate;
using Photon.Pun;

[VehicleScript(ScriptType.playerDriverScript)]

public class CarCheckpoints : MonoBehaviour
{
    private int checkpoints = 0;
    private BasicCheckpoint bc;
    public Vector3 checkpointPos;
    private GamestateTracker gamestateTracker;
    // Start is called before the first frame update
    void Start()
    {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        if (gamestateTracker.teams.Get((short)GetComponent<VehicleManager>().teamId).driverId != PhotonNetwork.LocalPlayer.ActorNumber) enabled = false;
        bc = (BasicCheckpoint)FindObjectOfType(typeof(BasicCheckpoint));
        if (!bc) Debug.LogWarning("No BasicCheckpoint object could be found (this is fine in menus but BAD in game");
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Checkpoint")) {
            checkpoints++;
            checkpointPos = bc.NextCheckpoint(checkpointPos);
            bc.gameObject.transform.position = checkpointPos;
            TeamEntry team = gamestateTracker.teams.Get((short)gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber).teamId);
            team.checkpoint = (short)checkpoints;
            team.Increment();
            Debug.Log(team.checkpoint);
        }
    }
}
