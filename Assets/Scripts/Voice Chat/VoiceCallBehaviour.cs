using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using Photon.Pun;
using Gamestate;

public class VoiceCallBehaviour : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void initialize(string id);

    [DllImport("__Internal")]
    private static extern void join(string id);

    [DllImport("__Internal")]
    private static extern void call(string id);

    GamestateTracker gamestateTracker;
    List<string> callerIDs;

    private void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        callerIDs = new List<string>();
        initialize($"{PhotonNetwork.CurrentRoom.Name}{PhotonNetwork.LocalPlayer.ActorNumber}");
    }

    public void JoinPeerJSSession() {
        for (int i = 0; i < gamestateTracker.players.count; i++) {
            PlayerEntry player = gamestateTracker.players.GetAtIndex(i);
            if (player.id != PhotonNetwork.LocalPlayer.ActorNumber) { // Don't want to call yourself
                join($"{PhotonNetwork.CurrentRoom.Name}{player.id}");
                callerIDs.Add($"{PhotonNetwork.CurrentRoom.Name}{player.id}");
            }
            player.Release();
        }
        JoinVoiceCall();
    }

    void JoinVoiceCall() {
        foreach (string callerID in callerIDs) {
            call(callerID);
        }
    }
}
