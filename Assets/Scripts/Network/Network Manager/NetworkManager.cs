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
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    ArrowBehaviour arrowBehaviour;

    public float gamespeed = 1f;
    public int maxPlayerPairs = 24;
    
    public List<Transform> spawnPoints;

    TimerBehaviour timer;

    public string version = "1.0";
    public string roomName = "room";

    public string defaultPlayerVehiclePrefabName;

    public List<string> defaultPlayerVehiclePrefabNames = new List<string>();

    private int loadedPlayers = 0;
    private int instantiatedPlayerIndex = 0;

    public GameObject loadingScreenPrefab;

    private GameObject loadingScreenInstance;
    
    

    public GameObject spawningPlayersScreenPrefab;
    
    private GameObject spawningPlayersScreenInstance;

    public GameObject startCountdownPrefab;
    private GameObject startCountdownInstance;
    
    public Transform testCube;
    public Transform testSphere;
    public float testCubeHeightThreshold =  -8.3f;

    public float testSphereHeightThreshold =  -7f;

    private void Awake() {
        Time.timeScale = gamespeed;
        Time.fixedDeltaTime = Time.timeScale * .02f;
    }
    
    // called on the master client when the game is fully set up
    void GameFullySetupMaster()
    {
        
        // remove overlay
        GetComponent<PhotonView>().RPC(nameof(RemoveSpawningPlayersCanvas), RpcTarget.AllBufferedViaServer);
        GetComponent<PhotonView>().RPC(nameof(StartCountdown), RpcTarget.AllBufferedViaServer);
        // start scoreboard stuff
        
        // activate all cars in time
        Invoke(nameof(ActivateVehicles), 4f);


        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        loadingScreenInstance = Instantiate(loadingScreenPrefab, transform.position, Quaternion.identity);
        Invoke(nameof(Begin), 3f);
        // Invoke(nameof(TestPhysics), 4f);
    }

    void Begin()
    {
        
        GetComponent<PhotonView>().RPC(nameof(LoadedMap), RpcTarget.AllBufferedViaServer);
        
    }
    void TestPhysics(){
        if(testCube.position.y > testCubeHeightThreshold || testSphere.position.y > testSphereHeightThreshold){
            GetComponent<PhotonView>().RPC(nameof(RequestReset), RpcTarget.AllBufferedViaServer);
        }
    }

    [PunRPC]
    void LoadedMap()
    {
        Debug.Log("loaded map");
        loadedPlayers += 1;
        if (loadedPlayers == PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC(nameof(AllPlayersLoaded), RpcTarget.AllBufferedViaServer);
        }
        
    }
    [PunRPC]
    void RequestReset(){
        if(PhotonNetwork.IsMasterClient){
            Debug.LogError("Physics is gonna be ffed up, reloading scene");
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }
        
    }

    [PunRPC]
    void AllPlayersLoaded()
    {
        // start instantiating the players
        if(loadingScreenInstance !=null) Destroy(loadingScreenInstance);
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
        // change back
        StartCoroutine(SpawnPlayers());
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


    IEnumerator SpawnPlayers()
    { 
        spawningPlayersScreenInstance = Instantiate(spawningPlayersScreenPrefab, transform.position, Quaternion.identity);
      //  yield return new WaitForSecondsRealtime(0.5f);
      //  if(FindObjectOfType<MakeTheMap>() != null) FindObjectOfType<MakeTheMap>().MakeMap();
        yield return new WaitForSecondsRealtime(1f);
        
        if (PhotonNetwork.IsMasterClient)
        {
            GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();

            
            // players should have already had their teams validated through the lobby screen
            // If we end up with bugs, get Jordan to add extra checks to fill slots with bots at this point.



            for (short i = 0; i < gamestateTracker.teams.count; i++)
            {
                TeamEntry entry = gamestateTracker.teams.GetAtIndex(i);
                int teamId = entry.id;
                entry.Release();
                // instantiate the vehicle from the vehiclePrefabName in the schema, if null, instantiate the testing truck
                Spawn(teamId);
                yield return new WaitForSeconds(0.5f);
            }
            
            GameFullySetupMaster();
        }
    }

 

    void ActivateVehicles()
    {
        NetworkPlayerVehicle[] npvs = FindObjectsOfType<NetworkPlayerVehicle>();
        foreach (NetworkPlayerVehicle npv in npvs)
        {
            npv.GetComponent<PhotonView>().RPC(nameof(NetworkPlayerVehicle.ActivateVehicleInputs), RpcTarget.AllBufferedViaServer);
        }
        if (FindObjectOfType<ArrowBehaviour>() != null) {
            arrowBehaviour = FindObjectOfType<ArrowBehaviour>();
            arrowBehaviour.ReadyUp();
        }
    }

    [PunRPC]
    void StartCountdown()
    {
        
        startCountdownInstance = Instantiate(startCountdownPrefab, transform.position, transform.rotation);
    }
    

    [PunRPC]
    void RemoveSpawningPlayersCanvas()
    {
        ScoreboardBehaviour sb = FindObjectOfType<ScoreboardBehaviour>();
        sb.StartScoreboard();
        if(spawningPlayersScreenInstance!=null) Destroy(spawningPlayersScreenInstance);
        
        
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
        yield return new WaitForSecondsRealtime(time);
        StartCoroutine(ResetVehicle(teamId));
    }

    IEnumerator ResetVehicle(int teamId) {
        List<VehicleHealthManager> vehicles = FindObjectsOfType<VehicleHealthManager>().ToList();
        foreach (VehicleHealthManager vehicle in vehicles) {
            if (vehicle.teamId == teamId) {
                // Reset stats
                vehicle.ResetProperties();



                // Move to spawnpoint
                vehicle.GetComponent<InputDriver>().enabled = true;
                Transform spawnPoint;
                if (teamId > spawnPoints.Count) {
                    spawnPoint = spawnPoints[0];
                } else {
                    spawnPoint = spawnPoints[teamId - 1];
                }
                PhysXRigidBody rigidBody = vehicle.GetComponent<PhysXRigidBody>();
                rigidBody.position = spawnPoint.position;
                rigidBody.rotation = spawnPoint.rotation;

                // Add back damping on camera after move
                yield return new WaitForSecondsRealtime(0.5f);

            }
        }
    }

    void Spawn(int teamId)
    {
        GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();
        TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);
        bool selected = teamEntry.hasSelectedVehicle;
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

        string vehiclePrefabName = defaultPlayerVehiclePrefabNames[Random.Range(0, defaultPlayerVehiclePrefabNames.Count)];
        
        
        if (selected) {
            vehiclePrefabName = "VehiclePrefabs/" + vehicleNames[vehicle];
        }

        object[] instantiationData = new object[]{teamId};

        //Put strong brakes on for spawn
        var spawnedVehicle = PhotonNetwork.Instantiate(vehiclePrefabName, sp.position, sp.rotation, 0, instantiationData);
        /*
        PhysXWheelCollider[] wheelColliders = spawnedVehicle.GetComponentsInChildren<PhysXWheelCollider>();
        foreach (PhysXWheelCollider wc in wheelColliders) {
            wc.brakeTorque = 10000;
        }*/
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("newPlayerJoined");
    }
    
    
}
