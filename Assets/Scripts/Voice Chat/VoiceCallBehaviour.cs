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
    private static extern void call(string id);

    GamestateTracker gamestateTracker;

    private void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
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
                call(callerID);
            }
            player.Release();
        }
    }

}
