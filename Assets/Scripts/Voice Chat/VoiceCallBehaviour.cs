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
        PlayerEntry self = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        self.isInVC = true;
        self.Increment();
        gameObject.GetComponent<Button>().interactable = false;
        gameObject.GetComponentInChildren<Text>().text = "Joined Call";
        for (int i = 0; i < gamestateTracker.players.count; i++) {
            PlayerEntry player = gamestateTracker.players.GetAtIndex(i);
            if (player.id != PhotonNetwork.LocalPlayer.ActorNumber && player.isInVC) {
                string callerID = PhotonNetwork.CurrentRoom.Name + player.id.ToString();
                Debug.Log($"Attempting to call from Unity: {callerID}");
                call(callerID);
                //callerIDs.Add($"{PhotonNetwork.CurrentRoom.Name}{player.id}");
                Debug.Log($"Called from Unity: {callerID}");
            }
            player.Release();
        }
        //JoinVoiceCall();
    }

    

    /*void JoinVoiceCall() {
        foreach (string callerID in callerIDs) {
            call(callerID);
            Debug.Log($"Trying to join call with ID: {callerID}");
        }
    }*/
}
