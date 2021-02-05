using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public List<Transform> spawnPoints;
    public Text statusText;

    public string version = "1.0";
    public string roomName = "room";

    public string playerVehiclePrefabName;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    void Update()
    {
        statusText.text = PhotonNetwork.NetworkClientState.ToString();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        RoomOptions options = new RoomOptions() {IsVisible = true, MaxPlayers = 16};
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        
        
        
        
    }
    
    // spawn each player pair at a respective spawnpoint
    // to do this, loop through each player in the gamestate tracker and get a list of the unique teams
    // once we have this, get the driver and gunner from both.
    // if a real player driver/gunner is missing, then instantiate a bot player (dummy for now)
    // to instantiate a bot player, add a new record to the gamestate tracker via server buffered rpc
    // instantiate the driver's vehicle for each of them (driver character)
    // instantiate the gunner attached to the vehicle for each of them (gunner character)
    
    // only to be called by the master client when we can be sure that everyone has loaded into the game
    public void spawnPlayers()
    {
        GamestateTracker tracker = FindObjectOfType<GamestateTracker>();
        List<GamestateTracker.PlayerDetails> playerDetailsList = tracker.playerList;
        List<int> uniqueTeamIds = new List<int>();
        // iterate through list to get all of the team ids
        foreach (GamestateTracker.PlayerDetails record in tracker.playerList)
        {
            if (!uniqueTeamIds.Contains(record.teamId))
            {
                uniqueTeamIds.Add(record.teamId);
            }
        }
        // get a gunner and driver from each unique team, compiling a list of player detail pairs
        List<List<GamestateTracker.PlayerDetails>> playerDetailsPairs = new List<List<GamestateTracker.PlayerDetails>>();
        foreach (int team in uniqueTeamIds)
        {
            // search our current team for players belonging to team i
            List<GamestateTracker.PlayerDetails> pair = new List<GamestateTracker.PlayerDetails>();
            foreach (GamestateTracker.PlayerDetails record in tracker.playerList)
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
        
        // we now have a list of players in teams
        
        

    }
    
    
    public void spawnPlayer()
    {
        
        
        
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count - 1)];
        GameObject myPlayer =
            PhotonNetwork.Instantiate(playerVehiclePrefabName,sp.position, sp.rotation);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("newPlayerJoined");
    }
    
    
}
