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

    GamestateTracker gamestateTracker;

    private void Start() {
#if UNITY_WEBGL && !UNITY_EDITOR
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        initializeA($"{PhotonNetwork.CurrentRoom.Name}{PhotonNetwork.LocalPlayer.ActorNumber}");
#else
        gameObject.GetComponent<Button>().interactable = false;
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "VC Disabled";
#endif
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
                callA(callerID);
            }
            player.Release();
        }
    }
}
