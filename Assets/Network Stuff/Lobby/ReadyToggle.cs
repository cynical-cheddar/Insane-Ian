using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ReadyToggle : MonoBehaviour
{

    public LobbySlotMaster lobbySlotMaster;

    public Toggle toggle;
    // Start is called before the first frame update
    void Start()
    {
        if (lobbySlotMaster == null)
        {
            lobbySlotMaster = FindObjectOfType<LobbySlotMaster>();
        }
    }

    public void changeReadyStatus()
    {
        
        bool state = toggle.isOn;
        
        // check if it is valid to ready up. If we have not selected a slot, set to false
        if (toggle.isOn && !lobbySlotMaster.getHasPicked())
        {
            toggle.isOn = false;
        }
        
        
        if(state)lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeReadyPlayers", RpcTarget.AllBufferedViaServer, 1);
        else lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeReadyPlayers", RpcTarget.AllBufferedViaServer, -1);
        
    }

    public void setReadyStatus(bool set)
    {
        if (set == false && toggle.isOn)
        {
            lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeReadyPlayers", RpcTarget.AllBufferedViaServer, 0);
        }
        else if (set == true && !toggle.isOn)
        {
            lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeReadyPlayers", RpcTarget.AllBufferedViaServer, 1);
        }
        
        toggle.isOn = set;
        
    }
}
