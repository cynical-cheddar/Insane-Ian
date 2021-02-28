using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Gamestate;

public class LobbyButtonScript : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    [SerializeField] public int teamId = 0 ;

    [SerializeField] public int driverPlayerId;

    [SerializeField] public int gunnerPlayerId;

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
        else if (!gunnerSlotEmpty && gunnerPlayerId == PhotonNetwork.LocalPlayer.ActorNumber)
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
        else if (!driverSlotEmpty && driverPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && lobbySlotMaster.getHasPicked())
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
            gunnerPlayerId = botDetails.playerId;
            GetComponent<PhotonView>().RPC("botSelectGunner", RpcTarget.AllBufferedViaServer, gunnerPlayerId);
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
            driverPlayerId = botDetails.playerId;
            GetComponent<PhotonView>().RPC("botSelectDriver", RpcTarget.AllBufferedViaServer, driverPlayerId);
        }
    }

    public void RemoveBotGunner()
    {
        if (!gunnerSlotEmpty&& PhotonNetwork.IsMasterClient)
        {
            GamestateTracker.PlayerDetails pd = gamestateTracker.getPlayerDetails(gunnerPlayerId);
            if(pd.isBot)GetComponent<PhotonView>().RPC("botDeselectGunner", RpcTarget.AllBufferedViaServer, gunnerPlayerId);
        }
    }

    public void RemoveBotDriver()
    {
        if (!driverSlotEmpty&& PhotonNetwork.IsMasterClient)
        {
            // check if the driver is a bot
            GamestateTracker.PlayerDetails pd = gamestateTracker.getPlayerDetails(driverPlayerId);
            
            if(pd.isBot)GetComponent<PhotonView>().RPC("botDeselectDriver", RpcTarget.AllBufferedViaServer, driverPlayerId);
        }
    }
    


    [PunRPC]
    void botSelectGunner(int botId)
    {
        addBotGunner.enabled = false;
        kickBotGunner.enabled = true;
        gunnerPlayerId = botId;
        gunnerSlotEmpty = false;

        GamestateTracker.PlayerDetails botDetails = gamestateTracker.getPlayerDetails(botId);
        botDetails.role = "Gunner";
        botDetails.teamId = teamId;
        gamestateTracker.UpdatePlayerWithNewRecord(botId, JsonUtility.ToJson(botDetails));

        gunnerPlayerText.text = gamestateTracker.getPlayerDetails(botId).nickName;
        
    }
    [PunRPC]
    void botSelectDriver(int botId)
    {
        addBotDriver.enabled = false;
        kickBotDriver.enabled = true;
        driverPlayerId = botId;
        driverSlotEmpty = false;

        GamestateTracker.PlayerDetails botDetails = gamestateTracker.getPlayerDetails(botId);
        botDetails.role = "Driver";
        botDetails.teamId = teamId;
        gamestateTracker.UpdatePlayerWithNewRecord(botId, JsonUtility.ToJson(botDetails));

        driverPlayerText.text = gamestateTracker.getPlayerDetails(botId).nickName;
        
    }
    [PunRPC]
    void botDeselectDriver(int botId)
    {
        driverPlayerId = 0;
        driverSlotEmpty = true;
        driverPlayerText.text = "empty";
        gamestateTracker.RemovePlayerFromSchema(botId);
        addBotDriver.enabled = true;
        kickBotDriver.enabled = false;
    }

    [PunRPC]
    void botDeselectGunner(int botId)
    {
        gunnerPlayerId = 0;
        gunnerSlotEmpty = true;
        gunnerPlayerText.text = "empty";
        gamestateTracker.RemovePlayerFromSchema(botId);
        addBotGunner.enabled = true;
        kickBotGunner.enabled = false;
    }
    
    [PunRPC]
    public void selectDriver(Player selectPlayer)
    {
        // if we have not picked and nobody else has picked our slot, then pick it
        // clear the player's last slot
            //select the slot
           
            driverPlayerId = selectPlayer.ActorNumber;
            driverSlotEmpty = false;
            driverPlayerText.text = selectPlayer.NickName;

            GamestateTracker.PlayerDetails playerDetails = gamestateTracker.getPlayerDetails(selectPlayer.ActorNumber);
            playerDetails.role = "Driver";
            playerDetails.teamId = teamId;
            gamestateTracker.UpdatePlayerWithNewRecord(selectPlayer.ActorNumber, JsonUtility.ToJson(playerDetails));

            if(PhotonNetwork.IsMasterClient) lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeSelectedPlayers", RpcTarget.AllBufferedViaServer, 1);
    }
    [PunRPC]
    public void deselectDriver(Player deselectPlayer)
    {
        
        driverPlayerId = 0;
        driverSlotEmpty = true;
        driverPlayerText.text = "empty";
        
        if (PhotonNetwork.IsMasterClient)lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC(nameof(LobbySlotMaster.changeSelectedPlayers), RpcTarget.AllBufferedViaServer, -1);
        readyToggle.setReadyStatus(false);

        GamestateTracker.PlayerDetails playerDetails = gamestateTracker.getPlayerDetails(deselectPlayer.ActorNumber);
        playerDetails.role = "null";
        playerDetails.teamId = 0;
        gamestateTracker.UpdatePlayerWithNewRecord(deselectPlayer.ActorNumber, JsonUtility.ToJson(playerDetails));
    }

    [PunRPC]
    public void DeselectPlayerDriverById(int id)
    {
        driverPlayerId = 0;
        driverSlotEmpty = true;
        driverPlayerText.text = "empty";

        if (PhotonNetwork.LocalPlayer.ActorNumber == id)
        {
            if(lobbySlotMaster.getHasPicked())lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC(nameof(LobbySlotMaster.changeSelectedPlayers), RpcTarget.AllBufferedViaServer, -1);
            readyToggle.setReadyStatus(false);
            lobbySlotMaster.setHasPicked(false);
        }
        
        GamestateTracker.PlayerDetails playerDetails = gamestateTracker.getPlayerDetails(id);
        playerDetails.role = "null";
        playerDetails.teamId = 0;
        gamestateTracker.UpdatePlayerWithNewRecord(id, JsonUtility.ToJson(playerDetails));

    }
    [PunRPC]
    public void DeselectGunnerDriverById(int id)
    {
        gunnerPlayerId = 0;
        gunnerSlotEmpty = true;
        gunnerPlayerText.text = "empty";

        if (PhotonNetwork.LocalPlayer.ActorNumber == id)
        {
            if(lobbySlotMaster.getHasPicked())lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC(nameof(LobbySlotMaster.changeSelectedPlayers), RpcTarget.AllBufferedViaServer, -1);
            readyToggle.setReadyStatus(false);
            lobbySlotMaster.setHasPicked(false);
        }
        
        GamestateTracker.PlayerDetails playerDetails = gamestateTracker.getPlayerDetails(id);
        playerDetails.role = "null";
        playerDetails.teamId = 0;
        gamestateTracker.UpdatePlayerWithNewRecord(id, JsonUtility.ToJson(playerDetails));
        
        
    }

    public void RemoveBothPlayersFromTeam()
    {
        if (driverPlayerId != 0)
        {
            GamestateTracker.PlayerDetails pd = gamestateTracker.getPlayerDetails(driverPlayerId);
            if(pd.isBot) RemoveBotDriver();
            else GetComponent<PhotonView>().RPC(nameof(DeselectPlayerDriverById), RpcTarget.AllBufferedViaServer, driverPlayerId);
        }

        if (gunnerPlayerId != 0)
        {
            GamestateTracker.PlayerDetails pd = gamestateTracker.getPlayerDetails(gunnerPlayerId);
            if(pd.isBot) RemoveBotGunner();
            else GetComponent<PhotonView>().RPC(nameof(DeselectGunnerDriverById), RpcTarget.AllBufferedViaServer, gunnerPlayerId);
        }
        
        
        

    }
    
    [PunRPC]
    public void ClearDriverButton()
    {
        driverPlayerId = 0;
        driverSlotEmpty = true;
        driverPlayerText.text = "empty";
    }
    [PunRPC]
    public void ClearGunnerButton()
    {
        gunnerPlayerId = 0;
        gunnerSlotEmpty = true;
        gunnerPlayerText.text = "empty";
    }
    
    
    [PunRPC]
    public void selectGunner(Player selectPlayer)
    {
        // clear the player's last slot
            // get all other LobbyButtons and clear 
            //select the slot
         
            gunnerPlayerId = selectPlayer.ActorNumber;
            gunnerSlotEmpty = false;
            gunnerPlayerText.text = selectPlayer.NickName;
        // atomically update player state
        //  GamestateTracker.PlayerDetails newPd = gamestateTracker.GetPlayerDetails(selectPlayer);
        //  newPd.role = "Gunner";
        //  newPd.teamId = teamId;
        //  gamestateTracker.UpdatePlayerInSchema();

        GamestateTracker.PlayerDetails playerDetails = gamestateTracker.getPlayerDetails(selectPlayer.ActorNumber);
        playerDetails.role = "Gunner";
        playerDetails.teamId = teamId;
        gamestateTracker.UpdatePlayerWithNewRecord(selectPlayer.ActorNumber, JsonUtility.ToJson(playerDetails));

        if (PhotonNetwork.IsMasterClient)lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeSelectedPlayers", RpcTarget.AllBufferedViaServer, 1);
    }
    
    [PunRPC]
    public void deselectGunner(Player deselectPlayer)
    {
       
        gunnerPlayerId = 0;
        gunnerSlotEmpty = true;
        gunnerPlayerText.text = "empty";
        
        if (PhotonNetwork.IsMasterClient)lobbySlotMaster.gameObject.GetComponent<PhotonView>().RPC("changeSelectedPlayers", RpcTarget.AllBufferedViaServer, -1);
        readyToggle.setReadyStatus(false);

        GamestateTracker.PlayerDetails playerDetails = gamestateTracker.getPlayerDetails(deselectPlayer.ActorNumber);
        playerDetails.role = "null";
        playerDetails.teamId = 0;
        gamestateTracker.UpdatePlayerWithNewRecord(deselectPlayer.ActorNumber, JsonUtility.ToJson(playerDetails));

        
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
