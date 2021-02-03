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
