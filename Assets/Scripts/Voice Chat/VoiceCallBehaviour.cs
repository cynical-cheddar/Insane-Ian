using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using Photon.Pun;
using Gamestate;
using TMPro;

public class VoiceCallBehaviour : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void initializeA(string id);

    [DllImport("__Internal")]
    private static extern void callA(string id);

    [DllImport("__Internal")]
    private static extern void mute(string id);

    [DllImport("__Internal")]
    private static extern void muteAll();

    GamestateTracker gamestateTracker;
    bool teammateIsBot;

    public PhotonView myPhotonView;

    private void Start() {
        // if in webgl, initialize the voice chat. Otherwise, disable it.
#if UNITY_WEBGL && !UNITY_EDITOR
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        initializeA($"{PhotonNetwork.CurrentRoom.Name}{PhotonNetwork.LocalPlayer.ActorNumber}");
#else
        gameObject.GetComponent<Button>().interactable = false;
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "VC Disabled";
#endif
    }

    // Called when Join Voice Call button is pressed. Calls all the players already in the VC.
    public void JoinPeerJSSession() {
        PlayerEntry self = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        self.isInVC = true;
        self.Increment();
        gameObject.GetComponent<Button>().interactable = false;
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Joined Call";
        for (int i = 0; i < gamestateTracker.players.count; i++) {
            PlayerEntry player = gamestateTracker.players.GetAtIndex(i);
            if (player.id != PhotonNetwork.LocalPlayer.ActorNumber && player.isInVC) {
                string callerID = PhotonNetwork.CurrentRoom.Name + player.id.ToString();
                callA(callerID);
            }
            player.Release();
        }
    }

    public void SeparateIntoTeams() {
        myPhotonView.RPC(nameof(SeparateIntoTeams_RPC), RpcTarget.All);
    }


    // Mute every player that isn't your teammate.
    [PunRPC]
    void SeparateIntoTeams_RPC() {
        #if UNITY_WEBGL && !UNITY_EDITOR
        PlayerEntry self = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        TeamEntry team = gamestateTracker.teams.Get(self.teamId);
        short teammateID = 0;
        if (team.gunnerId == self.id) {
            PlayerEntry partner = gamestateTracker.players.Get(team.driverId);
            if (partner.isBot) {
                teammateIsBot = true;
            } else {
                teammateID = partner.id;
            }
            partner.Release();
        }
        if (team.driverId == self.id) {
            PlayerEntry partner = gamestateTracker.players.Get(team.gunnerId);
            if (partner.isBot) {
                teammateIsBot = true;
            } else {
                teammateID = partner.id;
            }
            partner.Release();
        }
        self.Release();
        if (!teammateIsBot) {
            string vcID = PhotonNetwork.CurrentRoom.Name + teammateID.ToString();
            mute(vcID);
        } else {
            muteAll();
        }
        #endif
    }
}
