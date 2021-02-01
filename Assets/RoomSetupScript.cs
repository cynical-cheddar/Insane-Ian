using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class RoomSetupScript : MonoBehaviourPunCallbacks
{
    private int maxPlayers = 2;
    private string roomName = "room";
    public string mainLobbySceneName = "";

    public Text observedMaxPlayersText;
    public Text roomNameText;

    private void Start()
    {
        Random rnd = new Random();
        roomName = roomName + rnd.Next(1,9999).ToString();
    }

    public void SetMaxPlayers(int newMaxPlayers)
    {
        maxPlayers = newMaxPlayers;
    }

    public void SetRoomName(string newRoomName)
    {
        roomName = newRoomName;
    }
    // Start is called before the first frame update
    public void CreateRoomWithSettings()
    {
        // ideally, observe the text value of the max players
        if (observedMaxPlayersText) SetMaxPlayers(Int32.Parse(observedMaxPlayersText.text));
        if (roomNameText) SetRoomName(roomNameText.text);
        
        // now we have got the settings we need, create the room and load the main lobby scene
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = (byte) maxPlayers; //Set any number

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        // load the lobby scene for the room
        PhotonNetwork.LoadLevel(mainLobbySceneName);
    }


    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        //Load the Scene called GameLevel (Make sure it's added to build settings)
        PhotonNetwork.LoadLevel(mainLobbySceneName);
    }
}
