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
    public Text statusText;
    public TimerBehaviour timer;

    public string version = "1.0";
    public string roomName = "room";

    public string defaultPlayerVehiclePrefabName;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        startGame();
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
    
    public void startGame() {
        spawnPlayers();
        timer.hostStartTimer(300);
    }

    // spawn each player pair at a respective spawnpoint
    // to do this, loop through each player in the gamestate tracker and get a list of the unique teams
    // once we have this, get the driver and gunner from both.

    // instantiate the driver's vehicle for each of them (driver character)
    // instantiate the gunner attached to the vehicle for each of them (gunner character)
    
    // only to be called by the master client when we can be sure that everyone has loaded into the game
    public void spawnPlayers()
    {
        GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>(); 
        gamestateTracker.ForceSynchronisePlayerList();
        Invoke(nameof(actuallySpawn), 0.5f);
    }

    void actuallySpawn()
    { 
        
        if (PhotonNetwork.IsMasterClient)
        {
            GamestateTracker tracker = FindObjectOfType<GamestateTracker>();
            List<GamestateTracker.PlayerDetails> playerDetailsList = tracker.schema.playerList;
            List<List<GamestateTracker.PlayerDetails>> playerDetailsPairs = tracker.GetPlayerPairs();
            
            
            // players should have already had their teams validated through the lobby screen
            // If we end up with bugs, get Jordan to add extra checks to fill slots with bots at this point.


            // we now have a list of the players in each team
            foreach (List<GamestateTracker.PlayerDetails> playersPair in playerDetailsPairs)
            {
                // instantiate the vehicle from the vehiclePrefabName in the schema, if null, instantiate the testing truck
                GameObject vehicle = new GameObject();
                Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count - 1)];
                if (!(playersPair[0].vehiclePrefabName == "null" || playersPair[0].vehiclePrefabName == null ||
                      playersPair[0].vehiclePrefabName == ""))
                    vehicle = PhotonNetwork.Instantiate(playersPair[0].vehiclePrefabName, sp.position, sp.rotation);
                else vehicle = PhotonNetwork.Instantiate(defaultPlayerVehiclePrefabName, sp.position, sp.rotation);

                // on the testing truck, get the vehicle network controller script and set the pair details
                // when this is assigned, a method on the vehicle's script will enable/disable the appropriate scripts
                // it takes the player pair as an argument
                // the method is called AssignPairDetailsToVehicle(string serializedJson);
                Debug.Log("serialized pair size" + playersPair.Count.ToString());
                string serializedPlayer1 = JsonUtility.ToJson(playersPair[0]);
                Debug.Log("serialized 1: " + serializedPlayer1);
                string serializedPlayer2 = JsonUtility.ToJson(playersPair[1]);
                Debug.Log("serialized 2: " + serializedPlayer2);
                vehicle.GetComponent<PhotonView>().RPC(nameof(NetworkPlayerVehicle.AssignPairDetailsToVehicle),
                    RpcTarget.AllBufferedViaServer, serializedPlayer1, serializedPlayer2);
            }
        }
    }
    


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("newPlayerJoined");
    }
    
    
}
