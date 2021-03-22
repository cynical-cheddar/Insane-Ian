using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamestate;
using Photon.Pun;
using Photon.Realtime;

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
        bc = (BasicCheckpoint)FindObjectOfType(typeof(BasicCheckpoint));
        if (!bc) Debug.LogWarning("No BasicCheckpoint object could be found (this is fine in menus but BAD in game");
    }

    private void OnTriggerEnter(Collider other) {
        TeamEntry team = gamestateTracker.teams.Get((short)GetComponent<VehicleManager>().teamId);
        if (other.CompareTag("Checkpoint") && team.driverId == PhotonNetwork.LocalPlayer.ActorNumber) {
            checkpoints++;
            checkpointPos = bc.NextCheckpoint(checkpointPos);
            bc.gameObject.transform.position = checkpointPos;
            bc.photonView.RPC(nameof(BasicCheckpoint.UpdatePosition_RPC), RpcTarget.All, checkpointPos.x, checkpointPos.y, checkpointPos.z, team.gunnerId);
            team.checkpoint = (short)checkpoints;
            team.Increment();
        } else {
            team.Release();
        }
    }
}
