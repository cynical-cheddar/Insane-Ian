using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Gamestate;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public int maxPlayerPairs = 24;
    
    public List<Transform> spawnPoints;

    TimerBehaviour timer;

    public string version = "1.0";
    public string roomName = "room";

    public string defaultPlayerVehiclePrefabName;


    

    // Start is called before the first frame update
    void Start()
    {
        //PhotonNetwork.ConnectUsingSettings();
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
        GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();
        //SynchroniseSchemaBeforeSpawn();
        SpawnPlayers();
        timer = FindObjectOfType<TimerBehaviour>();
        GlobalsEntry globals = gamestateTracker.globals;
        float time = globals.timeLimit;
        globals.Release();
        if (timer != null) timer.HostStartTimer(time);
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
            //List<GamestateTracker.PlayerDetails> playerDetailsList = gamestateTracker.schema.playerList;
            //List<List<GamestateTracker.PlayerDetails>> playerPairs = gamestateTracker.GetPlayerPairs();
            
            
            // players should have already had their teams validated through the lobby screen
            // If we end up with bugs, get Jordan to add extra checks to fill slots with bots at this point.


            // we now have a list of the players in each team
            //foreach (GamestateTracker.TeamDetails team in gamestateTracker.schema.teamsList)
            for (short i = 0; i < gamestateTracker.teams.count; i++)
            {
                TeamEntry entry = gamestateTracker.teams.GetAtIndex(i);
                int teamId = entry.id;
                entry.Release();
                // instantiate the vehicle from the vehiclePrefabName in the schema, if null, instantiate the testing truck
                CallRespawnVehicle(0, teamId);
            }
        }
    }

    public void CallRespawnVehicle(float time, int teamId)
    {
        StartCoroutine(RespawnVehicle(time, teamId));
    }

    public void RespawnErrorHandler(TeamEntry teamEntry, bool succeeded) {
        if (teamEntry != null) {
            if (!succeeded && teamEntry.isDead) {
                teamEntry.isDead = false;
                teamEntry.Commit(RespawnErrorHandler);
            }
            else teamEntry.Release();
        }
    }

    IEnumerator RespawnVehicle(float time, int teamId) {
        GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();
        yield return new WaitForSecondsRealtime(time);
        
        //List<List<GamestateTracker.PlayerDetails>> playerPairs = gamestateTracker.GetPlayerPairs();
        /*GamestateTracker.TeamDetails team = gamestateTracker.getTeamDetails(teamId);
        
        // set dead = false for team 
        team.isDead = false;
        string serializedTeamJson = JsonUtility.ToJson(team);
        gamestateTracker.GetComponent<PhotonView>().RPC(nameof(GamestateTracker.UpdateTeamWithNewRecord), RpcTarget.AllBufferedViaServer, teamId, serializedTeamJson);*/
       // gamestateTracker.UpdateTeamWithNewRecord(teamId, serializedTeamJson);

        TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);
        teamEntry.isDead = false;
        short vehicle = teamEntry.vehicle;
        teamEntry.Commit(RespawnErrorHandler);
        
        Transform sp;
        if (teamId > spawnPoints.Count) {
            sp = spawnPoints[0];
        } else {
            sp = spawnPoints[teamId - 1];
        }

        GameObject[] vehicles = Resources.LoadAll<GameObject>("VehiclePrefabs");

        string vehiclePrefabName = defaultPlayerVehiclePrefabName;
        /*if (!(team.vehiclePrefabName == "null" || team.vehiclePrefabName == null ||
              team.vehiclePrefabName == ""))
                vehiclePrefabName = team.vehiclePrefabName;*/
        
        if (vehicle > 0) {
            vehiclePrefabName = "VehiclePrefabs/" + vehicles[vehicle - 1].name;
        }

        object[] instantiationData = new object[]{teamId};

        PhotonNetwork.Instantiate(vehiclePrefabName, sp.position, sp.rotation, 0, instantiationData);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("newPlayerJoined");
    }
    
    
}
