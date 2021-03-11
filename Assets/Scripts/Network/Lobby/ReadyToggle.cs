using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Gamestate;

public class ReadyToggle : MonoBehaviour
{

    public LobbySlotMaster lobbySlotMaster;
    GamestateTracker gamestateTracker;
    public Toggle toggle;
    // Start is called before the first frame update
    void Start()
    {
        if (lobbySlotMaster == null)
        {
            lobbySlotMaster = FindObjectOfType<LobbySlotMaster>();
        }

        gamestateTracker = FindObjectOfType<GamestateTracker>();
    }

    public void changeReadyStatus()
    {
        PlayerEntry playerEntry = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        
        bool state = toggle.isOn;
        
        // check if it is valid to ready up. If we have not selected a slot, set to false

        if (playerEntry.role == (short)PlayerEntry.Role.None) toggle.isOn = false;



        playerEntry.ready = toggle.isOn;
        playerEntry.Commit();
        
        lobbySlotMaster.GetComponent<PhotonView>().RPC(nameof(LobbySlotMaster.UpdateCountAndReady), RpcTarget.All);
        
    }

    public void setReadyStatus(bool set)
    {

        
        toggle.isOn = set;
        changeReadyStatus();

    }
}
