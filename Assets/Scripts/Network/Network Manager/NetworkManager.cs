using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Gamestate;
using System.Linq;
using Cinemachine;

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
        Invoke(nameof(SpawnPlayers), 2f);
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


    void SpawnPlayers()
    { 
        if (PhotonNetwork.IsMasterClient)
        {
            GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();
            //List<GamestateTracker.PlayerDetails> playerDetailsList = gamestateTracker.schema.playerList;
            //List<List<GamestateTracker.PlayerDetails>> playerPairs = gamestateTracker.GetPlayerPairs();
            
            
            // players should have already had their teams validated through the lobby screen
            // If we end up with bugs, get Jordan to add extra checks to fill slots with bots at this point.

            // a
            // we now have a list of the players in each team
            //foreach (GamestateTracker.TeamDetails team in gamestateTracker.schema.teamsList)
            for (short i = 0; i < gamestateTracker.teams.count; i++)
            {
                TeamEntry entry = gamestateTracker.teams.GetAtIndex(i);
                int teamId = entry.id;
                entry.Release();
                // instantiate the vehicle from the vehiclePrefabName in the schema, if null, instantiate the testing truck
                Spawn(teamId);
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

        //Spawn(teamId);
        StartCoroutine(ResetVehicle(teamId));
        //photonView.RPC(nameof(ResetVehicle_RPC), RpcTarget.All, teamId);
    }

    [PunRPC]
    void ResetVehicle_RPC(int teamId) {
        StartCoroutine(ResetVehicle(teamId));
    }

    IEnumerator ResetVehicle(int teamId) {
        List<VehicleManager> vehicles = FindObjectsOfType<VehicleManager>().ToList();
        foreach (VehicleManager vehicle in vehicles) {
            if (vehicle.teamId == teamId) {
                // Reset stats
                vehicle.ResetProperties();

                // Remove damping on camera before move
                List<List<float>> dampingValues = new List<List<float>>();
                CinemachineFreeLook driverCam = vehicle.GetComponentInChildren<DriverCameraBehaviour>().GetComponentInChildren<CinemachineFreeLook>();
                for (int i = 0; i < 3; i++) {
                    CinemachineOrbitalTransposer transposer = driverCam.GetRig(i).GetCinemachineComponent<CinemachineOrbitalTransposer>();
                    List<float> values = new List<float>();
                    values.Add(transposer.m_XDamping);
                    values.Add(transposer.m_YDamping);
                    values.Add(transposer.m_ZDamping);
                    values.Add(transposer.m_YawDamping);
                    dampingValues.Add(values);
                    transposer.m_XDamping = 0;
                    transposer.m_YDamping = 0;
                    transposer.m_ZDamping = 0;
                    transposer.m_YawDamping = 0;
                }

                // Move to spawnpoint
                vehicle.GetComponent<InputDriver>().enabled = true;
                Transform spawnPoint;
                if (teamId > spawnPoints.Count) {
                    spawnPoint = spawnPoints[0];
                } else {
                    spawnPoint = spawnPoints[teamId - 1];
                }
                vehicle.gameObject.transform.position = spawnPoint.position;
                vehicle.gameObject.transform.rotation = spawnPoint.rotation;

                // Add back damping on camera after move
                yield return new WaitForSecondsRealtime(0.5f);
                for (int i = 0; i < 3; i++) {
                    CinemachineOrbitalTransposer transposer = driverCam.GetRig(i).GetCinemachineComponent<CinemachineOrbitalTransposer>();
                    transposer.m_XDamping = dampingValues[i][0];
                    transposer.m_YDamping = dampingValues[i][1];
                    transposer.m_ZDamping = dampingValues[i][2];
                    transposer.m_YawDamping = dampingValues[i][3];
                }
                break;
            }
        }
    }

    void Spawn(int teamId)
    {
        GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();
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

        List<string> vehicleNames = gamestateTracker.GetComponent<GamestateVehicleLookup>().sortedVehicleNames;

        string vehiclePrefabName = defaultPlayerVehiclePrefabName;
        
        
        if (vehicle > 0) {
            vehiclePrefabName = "VehiclePrefabs/" + vehicleNames[vehicle];
        }

        object[] instantiationData = new object[]{teamId};

        PhotonNetwork.Instantiate(vehiclePrefabName, sp.position, sp.rotation, 0, instantiationData);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("newPlayerJoined");
    }
    
    
}
