using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyButtonScript : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    [SerializeField] private int teamId = 0;
    private Player driverPlayer;
    [SerializeField] private string driverPlayerNickName;
    private Player gunnerPlayer;
    [SerializeField] private string gunnerPlayerNickName;

    public bool driverSlotEmpty = true;
    public bool gunnerSlotEmpty = true;

    public Text driverPlayerText;
    public Text gunnerPlayerText;

    public ReadyToggle readyToggle;

    private LobbySlotMaster lobbySlotMaster;

    public GamestateTracker gamestateTracker;
    
    
    public void SetParent()
    {
        transform.parent = FindObjectOfType<LobbySlotMaster>().gameObject.transform;
    }

    public void clickGunner()
    {
        if (gunnerSlotEmpty && !lobbySlotMaster.getHasPicked())
        {
            lobbySlotMaster.setHasPicked(true);
            GetComponent<PhotonView>().RPC("selectGunner", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer);
        }
        else if (!gunnerSlotEmpty && gunnerPlayer.Equals(PhotonNetwork.LocalPlayer))
        {
            lobbySlotMaster.setHasPicked(false);
            GetComponent<PhotonView>().RPC("deselectGunner", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer);
        }
    }

    public void clickDriver()
    {
        if (driverSlotEmpty && !lobbySlotMaster.getHasPicked())
        {
            lobbySlotMaster.setHasPicked(true);
            GetComponent<PhotonView>().RPC("selectDriver", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer);
        }
        else if (!driverSlotEmpty && driverPlayer.Equals(PhotonNetwork.LocalPlayer) && lobbySlotMaster.getHasPicked())
        {
            lobbySlotMaster.setHasPicked(false);
            GetComponent<PhotonView>().RPC("deselectDriver", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer);
        }
       
    }

    [PunRPC]
    public void selectDriver(Player selectPlayer)
    {
        // if we have not picked and nobody else has picked our slot, then pick it
        // clear the player's last slot
            //select the slot
            driverPlayer = selectPlayer;
            driverPlayerNickName = selectPlayer.NickName;
            driverSlotEmpty = false;
            driverPlayerText.text = driverPlayerNickName;
            gamestateTracker.UpdatePlayerRole(selectPlayer, "Driver");
            gamestateTracker.UpdatePlayerTeam(selectPlayer, teamId);
            if(PhotonNetwork.IsMasterClient) lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeSelectedPlayers", RpcTarget.AllBufferedViaServer, 1);
    }
    [PunRPC]
    public void deselectDriver(Player deselectPlayer)
    {
        driverPlayer = null;
        driverPlayerNickName = "empty";
        driverSlotEmpty = true;
        driverPlayerText.text = "empty";
        gamestateTracker.UpdatePlayerRole(deselectPlayer, "null");
        gamestateTracker.UpdatePlayerTeam(deselectPlayer, 0);
        if(PhotonNetwork.IsMasterClient)lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeSelectedPlayers", RpcTarget.AllBufferedViaServer, -1);
        readyToggle.setReadyStatus(false);
    }
    
    
    [PunRPC]
    public void selectGunner(Player selectPlayer)
    {
        // clear the player's last slot
            // get all other LobbyButtons and clear 
            //select the slot
            gunnerPlayer = selectPlayer;
            gunnerPlayerNickName = selectPlayer.NickName;
            gunnerSlotEmpty = false;
            gunnerPlayerText.text = gunnerPlayerNickName;
            // atomically update player state
          //  GamestateTracker.PlayerDetails newPd = gamestateTracker.GetPlayerDetails(selectPlayer);
          //  newPd.role = "Gunner";
          //  newPd.teamId = teamId;
          //  gamestateTracker.UpdatePlayerInSchema();
            gamestateTracker.UpdatePlayerTeam(selectPlayer, teamId);
            gamestateTracker.UpdatePlayerRole(selectPlayer, "Gunner");
            
            if(PhotonNetwork.IsMasterClient)lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeSelectedPlayers", RpcTarget.AllBufferedViaServer, 1);
    }
    
    [PunRPC]
    public void deselectGunner(Player deselectPlayer)
    {
        gunnerPlayer = null;
        gunnerPlayerNickName = "empty";
        gunnerSlotEmpty = true;
        gunnerPlayerText.text = "empty";
        gamestateTracker.UpdatePlayerRole(deselectPlayer, "null");
        gamestateTracker.UpdatePlayerTeam(deselectPlayer, 0);
        if(PhotonNetwork.IsMasterClient)lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeSelectedPlayers", RpcTarget.AllBufferedViaServer, -1);
        readyToggle.setReadyStatus(false);
    }

    public void setButtonInfo(PlayerSchema playersInTeam)
    {
        foreach (PlayerSchema.Record record in playersInTeam.schema)
        {
            // check if we have a driver
            if (record.role == "Driver")
            {
                driverPlayerText.text = record.nickName;
            }
            // check if we have a driver
            if (record.role == "Gunner")
            {
                driverPlayerText.text = record.nickName;
            }
        }
    }

    void Start()
    {
        lobbySlotMaster = FindObjectOfType<LobbySlotMaster>();
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
