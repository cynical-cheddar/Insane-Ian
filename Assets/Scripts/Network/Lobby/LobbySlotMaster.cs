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
    public Text timeLimitText;

    public string selectedMap = "null";
    public string selectedMapDisplayName = "null";

    public string loadingSceneName = "loadingScene";
    // player, nickname, role, character
    
    public PlayerSchema playerSlotSchema = new PlayerSchema();

    public List<LobbyButtonScript> lobbyButtons = new List<LobbyButtonScript>();

    public GamestateTracker gamestateTracker;

    bool hasPicked = false;
    
    [PunRPC]
    void ChangeLobbyButtonActiveState_RPC(int index, bool state) {
        lobbyButtons[index].gameObject.SetActive(state);
        if (state) {
            gamestateTracker.schema.teamsList.Add(new GamestateTracker.TeamDetails(lobbyButtons[index].teamId));
        } else {
            // Scream
        }
    }

    public void AddTeam() {
        if (PhotonNetwork.IsMasterClient) {
            for (int i = 0; i < lobbyButtons.Count; i++) {
                if (!lobbyButtons[i].gameObject.activeInHierarchy) {
                    lobbyButtons[i].gameObject.SetActive(true);
                    GetComponent<PhotonView>().RPC(nameof(ChangeLobbyButtonActiveState_RPC), RpcTarget.OthersBuffered, i, true);
                    gamestateTracker.schema.teamsList.Add(new GamestateTracker.TeamDetails(lobbyButtons[i].teamId));
                    break;
                }
            }
        }
    }

    public void RemoveTeam() {
        if (PhotonNetwork.IsMasterClient) {
            for (int i = lobbyButtons.Count - 1; i >= 0; i--) {
                if (lobbyButtons[i].gameObject.activeInHierarchy) {
                    lobbyButtons[i].gameObject.SetActive(false);
                    GetComponent<PhotonView>().RPC(nameof(ChangeLobbyButtonActiveState_RPC), RpcTarget.OthersBuffered, i, false);
                    gamestateTracker.schema.teamsList.Remove(gamestateTracker.getTeamDetails(lobbyButtons[i].teamId));
                    break;
                }
            }
        }
    }

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

        foreach (LobbyButtonScript lobbyButton in lobbyButtons) {
            if (lobbyButton.gameObject.activeInHierarchy) {
                gamestateTracker.schema.teamsList.Add(new GamestateTracker.TeamDetails(lobbyButton.teamId));
            }
        }
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

    void FillRoleSlotWithBot(int teamId, string roleName) {
        GamestateTracker.PlayerDetails botDetails = gamestateTracker.generateBotDetails();
        botDetails.role = roleName;
        botDetails.teamId = teamId;
        fillSlotWithBot(botDetails);
        ForceUpdateLobbyButtonAddBot(botDetails);
    }

    // gets incomplete teams in the gamestate tracker and puts in a bot
    // only to be called by master client
    public void FillIncompleteTeamsWithBots() {
        GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();
        int currentBotNumber = gamestateTracker.GetNumberOfBotsInGame() + 1;
        foreach (GamestateTracker.TeamDetails team in gamestateTracker.schema.teamsList) {
            bool driverFilled = false;
            bool gunnerFilled = false;
            foreach (GamestateTracker.PlayerDetails player in gamestateTracker.schema.playerList) {
                if (player.teamId == team.teamId) {
                    if (player.role == "Driver") driverFilled = true;
                    if (player.role == "Gunner") gunnerFilled = true;
                }
                if (driverFilled && gunnerFilled) break;
            }
            if (!driverFilled) {
                FillRoleSlotWithBot(team.teamId, "Driver");
                currentBotNumber++;
            }
            if (!gunnerFilled) {
                FillRoleSlotWithBot(team.teamId, "Gunner");
                currentBotNumber++;
            }
        }
        gamestateTracker.ForceSynchronisePlayerSchema();
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
                FillIncompleteTeamsWithBots();
                if (timeLimitText.text != "") gamestateTracker.timeLimit = float.Parse(timeLimitText.text);
                PhotonNetwork.CurrentRoom.IsVisible = false;
                gamestateTracker.ForceSynchronisePlayerSchema();
                //Debug.Log("load new scene");
                // delayed load just to make sure sync and for Jordan to check the network update. Remove in build
                Invoke(nameof(delayedLoad), 0.1f);
            }
            else
            {
                Debug.Log("Players no ready or no map selected");
            }
        }
    }

    void delayedLoad()
    {
        gamestateTracker.ForceSynchronisePlayerSchema();
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
