using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public int maxPlayerPairs = 24;
    
    public List<Transform> spawnPoints;

    public TimerBehaviour timer;

    public string version = "1.0";
    public string roomName = "room";

    public string defaultPlayerVehiclePrefabName;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        StartGame();
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
    
    public void StartGame() {
        SynchroniseSchemaBeforeSpawn();
        if (timer != null) timer.HostStartTimer();
    }

    // spawn each player pair at a respective spawnpoint
    // to do this, loop through each player in the gamestate tracker and get a list of the unique teams
    // once we have this, get the driver and gunner from both.

    // instantiate the driver's vehicle for each of them (driver character)
    // instantiate the gunner attached to the vehicle for each of them (gunner character)
    
    // only to be called by the master client when we can be sure that everyone has loaded into the game
    public void SynchroniseSchemaBeforeSpawn()
    {
        GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>(); 
        gamestateTracker.ForceSynchronisePlayerSchema();
        Invoke(nameof(SpawnPlayers), 2f);
    }

    void SpawnPlayers()
    { 
        
        if (PhotonNetwork.IsMasterClient)
        {
            GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();
            List<GamestateTracker.PlayerDetails> playerDetailsList = gamestateTracker.schema.playerList;
            List<List<GamestateTracker.PlayerDetails>> playerPairs = gamestateTracker.GetPlayerPairs();
            
            
            // players should have already had their teams validated through the lobby screen
            // If we end up with bugs, get Jordan to add extra checks to fill slots with bots at this point.


            // we now have a list of the players in each team
            foreach (GamestateTracker.TeamDetails team in gamestateTracker.schema.teamsList)
            {
                // instantiate the vehicle from the vehiclePrefabName in the schema, if null, instantiate the testing truck
                GameObject vehicle = new GameObject();
                Transform sp;
                if (team.teamId > spawnPoints.Count) {
                    sp = spawnPoints[0];
                } else {
                    sp = spawnPoints[team.teamId - 1];
                }
                if (!(team.vehiclePrefabName == "null" || team.vehiclePrefabName == null ||
                      team.vehiclePrefabName == ""))
                     vehicle = PhotonNetwork.Instantiate(team.vehiclePrefabName, sp.position, sp.rotation);
                else vehicle = PhotonNetwork.Instantiate(defaultPlayerVehiclePrefabName, sp.position, sp.rotation);
                if (vehicle.GetComponent<VehicleManager>() != null)
                {
                    vehicle.GetComponent<VehicleManager>().teamId = team.teamId;
                }
                else
                {
                    Debug.LogError("Vehicle manager not found on vehicle prefab");
                }
               
                // on the testing truck, get the vehicle network controller script and set the pair details
                // when this is assigned, a method on the vehicle's script will enable/disable the appropriate scripts
                // it takes the player pair as an argument
                // the method is called AssignPairDetailsToVehicle(string serializedJson);
                //Debug.Log("serialized pair size" + playersPair.Count.ToString());
                foreach (List<GamestateTracker.PlayerDetails> playerPair in playerPairs) {
                    if (playerPair[0].teamId == team.teamId) {
                        string serializedPlayer1 = JsonUtility.ToJson(playerPair[0]);
                        //Debug.Log("serialized 1: " + serializedPlayer1);
                        string serializedPlayer2 = JsonUtility.ToJson(playerPair[1]);
                        //Debug.Log("serialized 2: " + serializedPlayer2);
                        vehicle.GetComponent<PhotonView>().RPC(nameof(NetworkPlayerVehicle.AssignPairDetailsToVehicle),
                            RpcTarget.AllBufferedViaServer, serializedPlayer1, serializedPlayer2);
                        break;
                    }
                }
            }
        }
    }

    public void CallRespawnVehicle(float time, int teamId)
    {
        StartCoroutine(respawnVehicle(time, teamId));
    }

    IEnumerator respawnVehicle(float time, int teamId) {
        GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();
        yield return new WaitForSecondsRealtime(time);
        
        List<List<GamestateTracker.PlayerDetails>> playerPairs = gamestateTracker.GetPlayerPairs();
        GamestateTracker.TeamDetails team = gamestateTracker.getTeamDetails(teamId);
        
        // set dead = false for team 
        team.isDead = false;
        string serializedTeamJson = JsonUtility.ToJson(team);
        gamestateTracker.GetComponent<PhotonView>().RPC(nameof(GamestateTracker.UpdateTeamWithNewRecord), RpcTarget.AllBufferedViaServer, teamId, serializedTeamJson);
       // gamestateTracker.UpdateTeamWithNewRecord(teamId, serializedTeamJson);
        
        GameObject vehicle = new GameObject();
        Transform sp;
        if (teamId > spawnPoints.Count) {
            sp = spawnPoints[0];
        } else {
            sp = spawnPoints[teamId - 1];
        }
        if (!(team.vehiclePrefabName == "null" || team.vehiclePrefabName == null ||
              team.vehiclePrefabName == ""))
            vehicle = PhotonNetwork.Instantiate(team.vehiclePrefabName, sp.position, sp.rotation);
        else vehicle = PhotonNetwork.Instantiate(defaultPlayerVehiclePrefabName, sp.position, sp.rotation);
        if (vehicle.GetComponent<VehicleManager>() != null)
        {
            vehicle.GetComponent<VehicleManager>().teamId = team.teamId;
        }
        foreach (List<GamestateTracker.PlayerDetails> playerPair in playerPairs) {
            if (playerPair[0].teamId == team.teamId) {
                string serializedPlayer1 = JsonUtility.ToJson(playerPair[0]);
                string serializedPlayer2 = JsonUtility.ToJson(playerPair[1]);
                vehicle.GetComponent<PhotonView>().RPC(nameof(NetworkPlayerVehicle.AssignPairDetailsToVehicle),  RpcTarget.AllBufferedViaServer, serializedPlayer1, serializedPlayer2);
                //break;
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("newPlayerJoined");
    }
    
    
}
