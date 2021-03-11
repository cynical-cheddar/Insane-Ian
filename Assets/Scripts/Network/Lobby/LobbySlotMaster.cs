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

    public GameObject teamSelectCanvas;
    public GameObject vehicleSelectCanvas;
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    // DEPRECATED STUFF
    
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

                    
                    // Add teams to new gamestate tracker
                    
                    lobbyButtons[i].GetComponent<LobbyButtonScript>().CreateTeamEntry();
                    
                    break;
                }
            }
        }
    }

    public void RemoveTeam() {
        if (PhotonNetwork.IsMasterClient) {
            for (int i = lobbyButtons.Count - 1; i >= 0; i--) {
                if (lobbyButtons[i].gameObject.activeInHierarchy && i > 0)
                {

                    
                    GetComponent<PhotonView>().RPC(nameof(ChangeLobbyButtonActiveState_RPC), RpcTarget.OthersBuffered, i, false);

                    
                    
                    
                    // new gamestate tracker stuff
                    // kick players
                    bool success = lobbyButtons[i].GetComponent<LobbyButtonScript>().TeamRemoveEntry();
                    
                    //set active false 
                    
                    if(success) lobbyButtons[i].gameObject.SetActive(false);
                    
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
        for (int i = 0; i < gamestateTracker.players.count; i++)
        {
            PlayerEntry playerEntry = gamestateTracker.players.GetAtIndex(i);
            if (playerEntry.isBot == false && playerEntry.ready) count++;
            playerEntry.Release();
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
        

        foreach (LobbyButtonScript lobbyButton in lobbyButtons) {
            if (lobbyButton.gameObject.activeInHierarchy) {
                //gamestateTracker.schema.teamsList.Add(new GamestateTracker.TeamDetails(lobbyButton.teamId));
                if(PhotonNetwork.IsMasterClient) lobbyButton.CreateTeamEntry();
                //TeamEntry team = gamestateTracker.teams.Create((short)lobbyButton.teamId);
                //team.Commit();
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
  
            PlayerEntry playerEntry = gamestateTracker.players.Create((short)newPlayer.ActorNumber);
            playerEntry.name = newPlayer.NickName;
            playerEntry.Commit();
        }
    }

    // gets incomplete teams in the gamestate tracker and puts in a bot
    // only to be called by master client
    public void FillIncompleteTeamsWithBots() {
        if (PhotonNetwork.IsMasterClient)
        {
            
            // foreach team in the gamestate tracker, check that valid ids exist for driver and gunner
            // if ids are 0 (ie not valid) then create a bot
            for (int i = 0; i < gamestateTracker.teams.count; i++)
            {
                TeamEntry teamEntry = gamestateTracker.teams.GetAtIndex(i);
                
                // add driver bot
                if (teamEntry.driverId == 0)
                {
                    PlayerEntry bot = gamestateTracker.players.Create(true, true);
                    bot.ready = true;
                    bot.role = (short) PlayerEntry.Role.Driver;
                    bot.isBot = true;
                    bot.name = "Bot " + -bot.id;
                    bot.teamId = (short) teamEntry.id;
                    bot.Commit();

                    // now add the entry to the team
                    teamEntry.driverId = bot.id;
                    teamEntry.Commit(); 
                }
                else
                {
                    teamEntry.Release();
                }
                
                teamEntry = gamestateTracker.teams.GetAtIndex(i);
                
                // add gunner bot
                if (teamEntry.gunnerId == 0)
                {
                    PlayerEntry bot = gamestateTracker.players.Create(true, true);
                    bot.ready = true;
                    bot.role = (short) PlayerEntry.Role.Gunner;
                    bot.isBot = true;
                    bot.name = "Bot " + -bot.id;
                    bot.teamId = (short) teamEntry.id;
                    bot.Commit();

                    // now add the entry to the team
                    teamEntry.gunnerId = bot.id;
                    teamEntry.Commit(); 
                }
                else
                {
                    teamEntry.Release();
                }
            }
        }
    }
    


    // Update is called once per frame


    // create a copy of the menu that the host has on their instance of the game absed on the host schema


    



    public void launchGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // check each team to make sure that they have selected 
            
            if (readyPlayers >= selectedPlayers && readyPlayers >= playersInLobby && selectedMap != "null")
            {
                GlobalsEntry globals = gamestateTracker.globals;
                if (timeLimitText.text != "") {
                    globals.timeLimit = short.Parse(timeLimitText.text);
                } else {
                    globals.timeLimit = 0;
                }
                globals.Commit();
                Invoke(nameof(delayedLoad), 0.1f);
                
            }
            else
            {
                Debug.Log("Players no ready or no map selected");
            }
        }
    }

    public void VehicleSelectScreen()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (readyPlayers >= selectedPlayers && readyPlayers >= playersInLobby && selectedMap != "null")
            {
                // get all info from lobby buttons and fill in the gametracker object
                FillIncompleteTeamsWithBots();
                if (timeLimitText.text != "") gamestateTracker.timeLimit = float.Parse(timeLimitText.text);
                PhotonNetwork.CurrentRoom.IsVisible = false;
                GetComponent<PhotonView>().RPC(nameof(delayedVehicleSelect), RpcTarget.All);
            }
            else
            {
                Debug.Log("Players no ready or no map selected");
            }
        }
    }

    [PunRPC]
    void delayedVehicleSelect()
    {
        teamSelectCanvas.SetActive(false);
        vehicleSelectCanvas.SetActive(true);
    }

    void delayedLoad()
    {
        gamestateTracker.ForceSynchronisePlayerSchema();
        PhotonNetwork.LoadLevel(loadingSceneName);
    }

 

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        
    }
}
