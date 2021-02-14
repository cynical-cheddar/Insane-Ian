using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbySlotMaster : MonoBehaviourPunCallbacks
{
    
    private int readyPlayers = 0;

    private int selectedPlayers = 0;

    private int playersInLobby = 1;

    private int maxPlayers = 0;

    public Text playersInLobbyText;
    public Text readyPlayersText;

    public string selectedMap = "null";
    public string selectedMapDisplayName = "null";

    public string loadingSceneName = "loadingScene";
    // player, nickname, role, character
    
    public PlayerSchema playerSlotSchema = new PlayerSchema();

    public List<LobbyButtonScript> lobbyButtons = new List<LobbyButtonScript>();

    public GamestateTracker gamestateTracker;

    bool hasPicked = false;
    
    public void setHasPicked(bool set)
    {
        hasPicked = set;
        // if it is true, add to the host's ability to 
    }

    [PunRPC]
    public void UpdateMapName(string sceneName, string sceneDisplayName)
    {
        selectedMap = sceneName;
        selectedMapDisplayName = sceneDisplayName;
    }
    [PunRPC]
    public void changeSelectedPlayers(int amt)
    {
        selectedPlayers += amt;
        // update the lobby stats on screen
        if(PhotonNetwork.IsMasterClient)GetComponent<PhotonView>().RPC("UpdateCountAndReady", RpcTarget.AllBufferedViaServer);
    }
    [PunRPC]
    public void changeReadyPlayers(int amt)
    {
        readyPlayers += amt;
        // update the lobby stats on screen
        if(PhotonNetwork.IsMasterClient)GetComponent<PhotonView>().RPC("UpdateCountAndReady", RpcTarget.AllBufferedViaServer);
    }
    public bool getHasPicked()
    {
        return hasPicked;
    }

    [PunRPC]
    public void UpdateCountAndReady()
    {
        playersInLobbyText.text = "Players in lobby:"  + playersInLobby.ToString();
        readyPlayersText.text = "Ready players: " + readyPlayers.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        
        maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        
        // add ourselves

        if (PhotonNetwork.IsMasterClient)
        {
            GamestateTracker.PlayerDetails pd = gamestateTracker.GenerateDefaultPlayerDetails(PhotonNetwork.LocalPlayer.NickName, PhotonNetwork.LocalPlayer.ActorNumber);
            
            gamestateTracker.GetComponent<PhotonView>().RPC("AddFirstPlayerToSchema", RpcTarget.AllBufferedViaServer, JsonUtility.ToJson(pd));
        }
        
        // update the lobby stats on screen
        if(PhotonNetwork.IsMasterClient)GetComponent<PhotonView>().RPC("UpdateCountAndReady", RpcTarget.AllBufferedViaServer);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        // update the lobby stats on screen
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("UpdateCountAndReady", RpcTarget.AllBufferedViaServer);
            // define the new player record
            GamestateTracker.PlayerDetails pd = gamestateTracker.GenerateDefaultPlayerDetails(newPlayer.NickName, newPlayer.ActorNumber);
            gamestateTracker.GetComponent<PhotonView>().RPC("AddPlayerToSchema", RpcTarget.AllBufferedViaServer, JsonUtility.ToJson(pd));
        }
    }

    // adds a bot to the player schema and also updates a button to make it taken
    
    public void fillSlotWithBot(GamestateTracker.PlayerDetails pd)
    {
        // checks to see if the entry may be added to the schema. 
        // The method on the gamestate tracker returns true if we have managed to successfully add the bot to the schema
        bool passedAddTests = gamestateTracker.mayAddBotToSchema(pd);

        if (passedAddTests)
        {
            // public void AddBotToSchema(string p, string role, string character, int team)
           // gamestateTracker.GetComponent<PhotonView>().RPC("AddPlayerToSchema", RpcTarget.AllBufferedViaServer, JsonUtility.ToJson(pd));
            gamestateTracker.AddBotToSchema(JsonUtility.ToJson((pd)));
        }
    }
    
    // method just updates graphical user interface
    void ForceUpdateLobbyButtonAddBot(GamestateTracker.PlayerDetails pd)
    {
        // search for buttons with corresponding team
        LobbyButtonScript[] lobbyButtonScripts = FindObjectsOfType<LobbyButtonScript>();
        foreach (LobbyButtonScript lbs in lobbyButtonScripts)
        {
            if (lbs.teamId == pd.teamId)
            {
                // if we are dealing with a gunner, do the graphics for adding a gunner bot
                if (pd.role == "Gunner")
                {
                    // call this botSelectGunner()
                    if(PhotonNetwork.IsMasterClient)lbs.gameObject.GetComponent<PhotonView>().RPC("botSelectGunner", RpcTarget.All, pd.playerId);
                }
                if (pd.role == "Driver")
                {
                    // call this botSelectDriver()
                    if(PhotonNetwork.IsMasterClient)lbs.gameObject.GetComponent<PhotonView>().RPC("botSelectDriver", RpcTarget.All, pd.playerId);
                }
            }
        }
    }

    // gets incomplete teams in the gamestate tracker and puts in a bot
    // only to be called by master client
    
    public void fillIncompleteTeamsWithBots()
    {
        GamestateTracker tracker = FindObjectOfType<GamestateTracker>();
        List<GamestateTracker.PlayerDetails> playerDetailsList = tracker.schema.playerList;
        List<int> uniqueTeamIds = new List<int>();
        // iterate through list to get all of the team ids
        foreach (GamestateTracker.PlayerDetails record in tracker.schema.playerList)
        {
            if (!uniqueTeamIds.Contains(record.teamId))
            {
                uniqueTeamIds.Add(record.teamId);
            }
        }
        Debug.Log("Unique teams: " + uniqueTeamIds.ToString());
        // get a gunner and driver from each unique team, compiling a list of player detail pairs
        List<List<GamestateTracker.PlayerDetails>> playerDetailsPairs = new List<List<GamestateTracker.PlayerDetails>>();
        foreach (int team in uniqueTeamIds)
        {
            // search our current team for players belonging to team i
            List<GamestateTracker.PlayerDetails> pair = new List<GamestateTracker.PlayerDetails>();
            foreach (GamestateTracker.PlayerDetails record in tracker.schema.playerList)
            {
                if (record.teamId == team)
                {
                    pair.Add(record);
                }
            }
            // avoid adding the null pair (it shouldn't exist, but it might)
            if (pair.Count > 0)
            {
                playerDetailsPairs.Add(pair);
            }
        }
        Debug.Log("playerDetailsPairs: " + playerDetailsPairs.ToString());
        // First bot details
       // GamestateTracker.PlayerDetails firstBotDetails = gamestateTracker.generateBotDetails();
        int currentBotNumber = gamestateTracker.GetNumberOfBotsInGame() + 1;
        // now we have a list of pairs, foreach free team space, bring in a bot
        foreach(List<GamestateTracker.PlayerDetails> pair in playerDetailsPairs)
        {
            // if there is only one player in a team, see what they are. Give them a friend
            if (pair.Count == 1)
            {
                // define a new bot
                GamestateTracker.PlayerDetails botDetails = gamestateTracker.generateBotDetails();
                //botDetails.nickName = "Bot: " + currentBotNumber.ToString();
                botDetails.teamId = pair[0].teamId;
                // check if the other player is a gunner or a driver
                if (pair[0].role == "Gunner") botDetails.role = "Driver";
                if (pair[0].role == "Driver") botDetails.role = "Gunner";
                // call the method to fill a slot with our defined bot details
                
                fillSlotWithBot(botDetails);
                currentBotNumber++;
                ForceUpdateLobbyButtonAddBot(botDetails);
            }   
        }
    }
    


    // Update is called once per frame


    // create a copy of the menu that the host has on their instance of the game absed on the host schema


    



    public void launchGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (readyPlayers == selectedPlayers && readyPlayers == playersInLobby && selectedMap != "null")
            {
                // get all info from lobby buttons and fill in the gametracker object
                fillIncompleteTeamsWithBots();
                gamestateTracker.ForceSynchronisePlayerSchema();
                Debug.Log("load new scene");
                // delayed load just to make sure sync and for Jordan to check the network update. Remove in build
                Invoke(nameof(delayedLoad), 2f);
            }
            else
            {
                Debug.Log("Players no ready or no map selected");
            }
        }
    }

    void delayedLoad()
    {
        PhotonNetwork.LoadLevel(loadingSceneName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("UpdateCountAndReady", RpcTarget.AllBufferedViaServer);
            gamestateTracker.GetComponent<PhotonView>().RPC("RemovePlayerFromSchema", RpcTarget.AllBufferedViaServer, otherPlayer.ActorNumber);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        
    }
}
