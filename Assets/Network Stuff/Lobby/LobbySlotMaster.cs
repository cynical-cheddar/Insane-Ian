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
        selectedMapDisplayName = selectedMapDisplayName;
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
        
        if(PhotonNetwork.IsMasterClient)gamestateTracker.GetComponent<PhotonView>().RPC("AddFirstPlayerToSchema", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer);
        
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
            gamestateTracker.GetComponent<PhotonView>().RPC("AddPlayerToSchema", RpcTarget.AllBufferedViaServer, newPlayer);
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
               
                Debug.Log("load new scene");
                PhotonNetwork.LoadLevel(loadingSceneName);
            }
            else
            {
                Debug.Log("Players no ready or no map selected");
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("UpdateCountAndReady", RpcTarget.AllBufferedViaServer);
            gamestateTracker.GetComponent<PhotonView>().RPC("RemovePlayerFromSchema", RpcTarget.AllBufferedViaServer, otherPlayer);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        
    }
}
