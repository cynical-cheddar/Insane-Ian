using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Gamestate;

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
                if (lobbyButtons[i].gameObject.activeInHierarchy)
                {
                    lobbyButtons[i].RemoveBothPlayersFromTeam();
                    
                    GetComponent<PhotonView>().RPC(nameof(ChangeLobbyButtonActiveState_RPC), RpcTarget.OthersBuffered, i, false);
                    // deselect both players from team
                    
                    gamestateTracker.schema.teamsList.Remove(gamestateTracker.getTeamDetails(lobbyButtons[i].teamId));
                    lobbyButtons[i].gameObject.SetActive(false);
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
        GetComponent<PhotonView>().RPC("UpdateCountAndReady", RpcTarget.AllBufferedViaServer);
    }
    [PunRPC]
    public void changeReadyPlayers(int amt)
    {
        gamestateTracker.ForceSynchronisePlayerSchema();
        GetComponent<PhotonView>().RPC("UpdateCountAndReady", RpcTarget.AllBufferedViaServer);
    }
    public bool getHasPicked()
    {
        return hasPicked;
    }

    [PunRPC]
    public void UpdateCountAndReady()
    {
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        int count = 0;
        // foreach player, sum the amount of readies
        foreach(GamestateTracker.PlayerDetails pd in gamestateTracker.schema.playerList)
        {
            if (pd.ready && !pd.isBot) count++;
        }
        
        
        readyPlayers = count;
        
        playersInLobbyText.text = "Players in lobby:"  + playersInLobby.ToString();
        readyPlayersText.text = "Ready players: " + readyPlayers.ToString();
    }

    void Update()
    {
        UpdateCountAndReady();
    }

    // Start is called before the first frame update
    void Start()
    {
        
        maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        
        // add ourselves

        if (PhotonNetwork.IsMasterClient)
        {
            PlayerEntry playerEntry = gamestateTracker.players.Create((short)PhotonNetwork.LocalPlayer.ActorNumber);
            playerEntry.name = PhotonNetwork.LocalPlayer.NickName;
            playerEntry.Commit();
        }
        
        // update the lobby stats on screen
        if(PhotonNetwork.IsMasterClient)GetComponent<PhotonView>().RPC("UpdateCountAndReady", RpcTarget.AllBufferedViaServer);

        foreach (LobbyButtonScript lobbyButton in lobbyButtons) {
            if (lobbyButton.gameObject.activeInHierarchy) {
                //gamestateTracker.schema.teamsList.Add(new GamestateTracker.TeamDetails(lobbyButton.teamId));
                gamestateTracker.teams.Create((short)lobbyButton.teamId);
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

            /*
            // define the new player record
            GamestateTracker.PlayerDetails pd = gamestateTracker.GenerateDefaultPlayerDetails(newPlayer.NickName, newPlayer.ActorNumber);
            gamestateTracker.GetComponent<PhotonView>().RPC("AddPlayerToSchema", RpcTarget.AllBufferedViaServer, JsonUtility.ToJson(pd));*/

            PlayerEntry playerEntry = gamestateTracker.players.Create((short)newPlayer.ActorNumber);
            playerEntry.name = newPlayer.NickName;
            playerEntry.Commit();
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
    void ForceUpdateLobbyButtonAddBot(PlayerEntry entry)
    {
        // search for buttons with corresponding team
        LobbyButtonScript[] lobbyButtonScripts = FindObjectsOfType<LobbyButtonScript>();
        foreach (LobbyButtonScript lbs in lobbyButtonScripts)
        {
            if (lbs.teamId == entry.id)
            {
                // if we are dealing with a gunner, do the graphics for adding a gunner bot
                if (entry.role == (short)PlayerEntry.Role.Gunner)
                {
                    // call this botSelectGunner()
                    if(PhotonNetwork.IsMasterClient)lbs.gameObject.GetComponent<PhotonView>().RPC("botSelectGunner", RpcTarget.All, entry.id);
                }
                if (entry.role == (short)PlayerEntry.Role.Driver)
                {
                    // call this botSelectDriver()
                    if(PhotonNetwork.IsMasterClient)lbs.gameObject.GetComponent<PhotonView>().RPC("botSelectDriver", RpcTarget.All, entry.id);
                }
            }
        }
    }

    void FillRoleSlotWithBot(int teamId, PlayerEntry.Role role) {
        /*GamestateTracker.PlayerDetails botDetails = gamestateTracker.generateBotDetails();
        botDetails.role = roleName;
        botDetails.teamId = teamId;
        fillSlotWithBot(botDetails);
        ForceUpdateLobbyButtonAddBot(botDetails);*/

        PlayerEntry botEntry = gamestateTracker.players.Create(true, true);
        botEntry.name = "Bot " + -botEntry.id;
        botEntry.role = (short)role;
        botEntry.teamId = (short)teamId;
        botEntry.isBot = true;
        botEntry.Commit((PlayerEntry entry, bool succeeded) => {
            if (succeeded) {
                ForceUpdateLobbyButtonAddBot(entry);
            }
            entry.Release();
        });
    }

    // gets incomplete teams in the gamestate tracker and puts in a bot
    // only to be called by master client
    public void FillIncompleteTeamsWithBots() {
        GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();
        //foreach (TeamEntry team in gamestateTracker.schema.teamsList) {
        for (short i = 0; i < gamestateTracker.teams.count; i++) {
            TeamEntry team = gamestateTracker.teams.GetAtIndex(i);
            bool driverFilled = false;
            bool gunnerFilled = false;
            //foreach (PlayerEntry player in gamestateTracker.schema.playerList) {
            for (short j = 0; j < gamestateTracker.players.count; j++) {
                PlayerEntry player = gamestateTracker.players.GetAtIndex(i);
                if (player.teamId == team.id) {
                    if (player.role == (short)PlayerEntry.Role.Driver) driverFilled = true;
                    if (player.role == (short)PlayerEntry.Role.Gunner) gunnerFilled = true;
                }
                if (driverFilled && gunnerFilled) break;
            }
            if (!driverFilled) {
                FillRoleSlotWithBot(team.id, PlayerEntry.Role.Driver);
            }
            if (!gunnerFilled) {
                FillRoleSlotWithBot(team.id, PlayerEntry.Role.Gunner);
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
            if (readyPlayers >= selectedPlayers && readyPlayers >= playersInLobby && selectedMap != "null")
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
            // boot the player from their slot
            // lookup the player from their tracker
            GamestateTracker.PlayerDetails leftPlayerDetails = gamestateTracker.getPlayerDetails(otherPlayer.ActorNumber);
            foreach (LobbyButtonScript lb in lobbyButtons)
            {
                // compare the driver and gunner ids
                if(lb.driverPlayerId == leftPlayerDetails.playerId) lb.GetComponent<PhotonView>().RPC(nameof(LobbyButtonScript.ClearDriverButton), RpcTarget.AllBufferedViaServer);
                if(lb.gunnerPlayerId == leftPlayerDetails.playerId) lb.GetComponent<PhotonView>().RPC(nameof(LobbyButtonScript.ClearGunnerButton), RpcTarget.AllBufferedViaServer);
            }
            
            
            GetComponent<PhotonView>().RPC(nameof(UpdateCountAndReady), RpcTarget.AllBufferedViaServer);
            gamestateTracker.GetComponent<PhotonView>().RPC(nameof(GamestateTracker.RemovePlayerFromSchema), RpcTarget.AllBufferedViaServer, otherPlayer.ActorNumber);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        
    }
}
