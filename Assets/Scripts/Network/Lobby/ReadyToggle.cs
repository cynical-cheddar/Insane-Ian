using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

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
        
        bool state = toggle.isOn;
        
        // check if it is valid to ready up. If we have not selected a slot, set to false
        if (toggle.isOn && !lobbySlotMaster.getHasPicked())
        {
            toggle.isOn = false;
        }
        
        // update gamestate tracker with ready id
        GamestateTracker.PlayerDetails pd = gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber);
        pd.ready = toggle.isOn;
        gamestateTracker.GetComponent<PhotonView>().RPC(nameof(GamestateTracker.UpdatePlayerWithNewRecord), RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber, JsonUtility.ToJson(pd)); 
        
    //    if(state)lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC(nameof(LobbySlotMaster.changeReadyPlayers), RpcTarget.AllBufferedViaServer, 1);
     //   else lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC(nameof(LobbySlotMaster.changeReadyPlayers), RpcTarget.AllBufferedViaServer, -1);
        lobbySlotMaster.changeReadyPlayers(0);
    }

    public void setReadyStatus(bool set)
    {

        
        toggle.isOn = set;
        changeReadyStatus();

    }
}
