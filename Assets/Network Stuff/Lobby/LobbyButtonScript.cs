using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyButtonScript : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    [SerializeField] public int teamId = 0 ;

    [SerializeField] private string driverPlayerNickName;

    [SerializeField] private string gunnerPlayerNickName;

    public bool driverSlotEmpty = true;
    public bool gunnerSlotEmpty = true;

    public Text driverPlayerText;
    public Text gunnerPlayerText;

    public ReadyToggle readyToggle;

    private LobbySlotMaster lobbySlotMaster;

    public GamestateTracker gamestateTracker;

    public Image addBotDriver;
    public Image kickBotDriver;

    public Image addBotGunner;
    public Image kickBotGunner;
    
    public void SetParent()
    {
        transform.parent = FindObjectOfType<LobbySlotMaster>().gameObject.transform;
    }
    // called when a player wants to be in this slot
    public void clickGunner()
    {
        if (gunnerSlotEmpty && !lobbySlotMaster.getHasPicked())
        {
            lobbySlotMaster.setHasPicked(true);
            GetComponent<PhotonView>().RPC("selectGunner", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer);
        }
        else if (!gunnerSlotEmpty && gunnerPlayerNickName.Equals(PhotonNetwork.LocalPlayer.NickName))
        {
            lobbySlotMaster.setHasPicked(false);
            GetComponent<PhotonView>().RPC("deselectGunner", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer);
        }
    }
    // called when a player wants to be this slot
    public void clickDriver()
    {
        if (driverSlotEmpty && !lobbySlotMaster.getHasPicked())
        {
            lobbySlotMaster.setHasPicked(true);
            GetComponent<PhotonView>().RPC("selectDriver", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer);
        }
        else if (!driverSlotEmpty && driverPlayerNickName.Equals(PhotonNetwork.LocalPlayer.NickName) && lobbySlotMaster.getHasPicked())
        {
            lobbySlotMaster.setHasPicked(false);
            GetComponent<PhotonView>().RPC("deselectDriver", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer);
        }
    }

    public void AddBotGunner()
    {
        if (gunnerSlotEmpty && PhotonNetwork.IsMasterClient)
        {
            // ask the lobbySlotMaster to add a bot to this slot
            GamestateTracker.PlayerDetails botDetails = gamestateTracker.generateBotDetails();
            botDetails.role = "Gunner";
            botDetails.teamId = teamId;
            lobbySlotMaster.fillSlotWithBot(botDetails);
            gunnerPlayerNickName = botDetails.nickName;
            GetComponent<PhotonView>().RPC("botSelectGunner", RpcTarget.AllBufferedViaServer, gunnerPlayerNickName);
        }
    }
    public void AddBotDriver()
    {
        if (driverSlotEmpty && PhotonNetwork.IsMasterClient)
        {
            // ask the lobbySlotMaster to add a bot to this slot
            GamestateTracker.PlayerDetails botDetails = gamestateTracker.generateBotDetails();
            botDetails.role = "Driver";
            botDetails.teamId = teamId;
            lobbySlotMaster.fillSlotWithBot(botDetails);
            driverPlayerNickName = botDetails.nickName;
            GetComponent<PhotonView>().RPC("botSelectDriver", RpcTarget.AllBufferedViaServer, driverPlayerNickName);
        }
    }

    public void RemoveBotGunner()
    {
        if (!gunnerSlotEmpty&& PhotonNetwork.IsMasterClient)
        {
            GamestateTracker.PlayerDetails pd = gamestateTracker.getPlayerDetails(gunnerPlayerNickName);
            if(pd.isBot)GetComponent<PhotonView>().RPC("botDeselectGunner", RpcTarget.AllBufferedViaServer, gunnerPlayerNickName);
        }
    }

    public void RemoveBotDriver()
    {
        if (!driverSlotEmpty&& PhotonNetwork.IsMasterClient)
        {
            // check if the driver is a bot
            GamestateTracker.PlayerDetails pd = gamestateTracker.getPlayerDetails(driverPlayerNickName);
            
            if(pd.isBot)GetComponent<PhotonView>().RPC("botDeselectDriver", RpcTarget.AllBufferedViaServer, driverPlayerNickName);
        }
    }
    


    [PunRPC]
    void botSelectGunner(string botName)
    {
        addBotGunner.enabled = false;
        kickBotGunner.enabled = true;
        gunnerPlayerNickName = botName;
        gunnerSlotEmpty = false;
        gunnerPlayerText.text = botName;
        
        gamestateTracker.UpdatePlayerRole(botName, "Gunner");
        gamestateTracker.UpdatePlayerTeam(botName, teamId);
        
    }
    [PunRPC]
    void botSelectDriver(string botName)
    {
        addBotDriver.enabled = false;
        kickBotDriver.enabled = true;
        driverPlayerNickName = botName;
        driverSlotEmpty = false;
        driverPlayerText.text = botName;
        gamestateTracker.UpdatePlayerRole(botName, "Driver");
        gamestateTracker.UpdatePlayerTeam(botName, teamId);
        
    }
    [PunRPC]
    void botDeselectDriver(string botName)
    {
        driverPlayerNickName = "empty";
        driverSlotEmpty = true;
        driverPlayerText.text = "empty";
        gamestateTracker.RemovePlayerFromSchema(botName);
        addBotDriver.enabled = true;
        kickBotDriver.enabled = false;
    }

    [PunRPC]
    void botDeselectGunner(string botName)
    {
        gunnerPlayerNickName = "empty";
        gunnerSlotEmpty = true;
        gunnerPlayerText.text = "empty";
        gamestateTracker.RemovePlayerFromSchema(botName);
        addBotGunner.enabled = true;
        kickBotGunner.enabled = false;
    }
    
    [PunRPC]
    public void selectDriver(Player selectPlayer)
    {
        // if we have not picked and nobody else has picked our slot, then pick it
        // clear the player's last slot
            //select the slot
           
            driverPlayerNickName = selectPlayer.NickName;
            driverSlotEmpty = false;
            driverPlayerText.text = driverPlayerNickName;
            gamestateTracker.UpdatePlayerRole(selectPlayer.NickName, "Driver");
            gamestateTracker.UpdatePlayerTeam(selectPlayer.NickName, teamId);
            if(PhotonNetwork.IsMasterClient) lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeSelectedPlayers", RpcTarget.AllBufferedViaServer, 1);
    }
    [PunRPC]
    public void deselectDriver(Player deselectPlayer)
    {
    
        driverPlayerNickName = "empty";
        driverSlotEmpty = true;
        driverPlayerText.text = "empty";
        gamestateTracker.UpdatePlayerRole(deselectPlayer.NickName, "null");
        gamestateTracker.UpdatePlayerTeam(deselectPlayer.NickName, 0);
        if(PhotonNetwork.IsMasterClient)lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeSelectedPlayers", RpcTarget.AllBufferedViaServer, -1);
        readyToggle.setReadyStatus(false);
    }
    
    
    [PunRPC]
    public void selectGunner(Player selectPlayer)
    {
        // clear the player's last slot
            // get all other LobbyButtons and clear 
            //select the slot
         
            gunnerPlayerNickName = selectPlayer.NickName;
            gunnerSlotEmpty = false;
            gunnerPlayerText.text = gunnerPlayerNickName;
            // atomically update player state
          //  GamestateTracker.PlayerDetails newPd = gamestateTracker.GetPlayerDetails(selectPlayer);
          //  newPd.role = "Gunner";
          //  newPd.teamId = teamId;
          //  gamestateTracker.UpdatePlayerInSchema();
            gamestateTracker.UpdatePlayerTeam(selectPlayer.NickName, teamId);
            gamestateTracker.UpdatePlayerRole(selectPlayer.NickName, "Gunner");
            
            if(PhotonNetwork.IsMasterClient)lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeSelectedPlayers", RpcTarget.AllBufferedViaServer, 1);
    }
    
    [PunRPC]
    public void deselectGunner(Player deselectPlayer)
    {
       
        gunnerPlayerNickName = "empty";
        gunnerSlotEmpty = true;
        gunnerPlayerText.text = "empty";
        gamestateTracker.UpdatePlayerRole(deselectPlayer.NickName, "null");
        gamestateTracker.UpdatePlayerTeam(deselectPlayer.NickName, 0);
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
