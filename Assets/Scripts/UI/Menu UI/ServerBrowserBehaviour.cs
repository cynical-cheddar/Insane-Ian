using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class ServerBrowserBehaviour : MonoBehaviourPunCallbacks {

    public Text statusText;
    public GameObject Page;
    public GameObject ExtraPagesInfo;
    public GameObject BrowserText;
    public TMP_InputField nameInputField;
    public Button NextPageButton;
    public Button PrevPageButton;
    public TextMeshProUGUI pageNumberText;

    int pageNumber = 0;
    int noOfPages = 1;
    int roomsPerPage;
    //Users are separated from each other by gameversion (which allows you to make breaking changes).
    string gameVersion = "0.9";
    List<RoomInfo> createdRooms = new List<RoomInfo>();
    PageBehaviour pageBehaviour;
    List<RoomPanelBehaviour> roomPBs;
    List<string> randomNames;

    void Start()
    {
        //This makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            //Set the App version before connecting
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings();
        }

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        roomPBs = new List<RoomPanelBehaviour>();
        pageBehaviour = Page.GetComponent<PageBehaviour>();
        foreach (GameObject room in pageBehaviour.RoomPanels) {
            roomPBs.Add(room.GetComponent<RoomPanelBehaviour>());
        }
        roomsPerPage = pageBehaviour.roomsPerPage;
        randomNames = new List<string>() {
            "XxInsaneIanxX",
            "Her Majesty The Queen TM",
            "James. Just James",
            "Not A Bot",
            "Didn't Choose A Nickname",
            "Get Good, Get LmaoBox",
            "Suave Announcer",
            "Unspecified Panel Member"
        };
    }

    void Update() {
        statusText.text = $"Status: {PhotonNetwork.NetworkClientState}";
    }

    public override void OnConnectedToMaster() {
        //Debug.Log("OnConnectedToMaster");
        //After we connected to Master server, join the Lobby
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby() {
        BrowserText.GetComponent<TextMeshProUGUI>().text = "There are no rooms available";
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        //Debug.Log("We have received the Room list");
        //After this callback, update the room list
        createdRooms = roomList;
        RefreshRoomPages();
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        Debug.LogError("OnJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.LogError("OnJoinRandomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    void RefreshRoomPages() {
        // Manage multiple pages
        noOfPages = Mathf.CeilToInt((float)createdRooms.Count / (float)roomsPerPage);
        while (pageNumber > noOfPages) {
            pageNumber--;
        }
        pageNumberText.text = $"Page: {pageNumber}";
        if (noOfPages > 1) {
            ExtraPagesInfo.gameObject.SetActive(true);
        } else {
            ExtraPagesInfo.gameObject.SetActive(false);
        }
        if (createdRooms.Count > 0) {
            Page.gameObject.SetActive(true);
            BrowserText.gameObject.SetActive(false);

            // Activate rooms on page
            int activeRoomsInPage = roomsPerPage;
            if (pageNumber == noOfPages - 1 || noOfPages == 1) { // On last page
                NextPageButton.interactable = false;
                PrevPageButton.interactable = true;
                if (createdRooms.Count % roomsPerPage != 0) {
                    activeRoomsInPage = createdRooms.Count % roomsPerPage;
                }
            } else if (pageNumber == 0) { // On first page
                NextPageButton.interactable = true;
                PrevPageButton.interactable = false;
            }
            int index = pageNumber * roomsPerPage;
            for (int i = 0; i < roomsPerPage; i++) {
                if (i < activeRoomsInPage) {
                    roomPBs[i].gameObject.SetActive(true);
                    roomPBs[i].roomName.text = createdRooms[index + i].Name;
                    roomPBs[i].playersNumber.text = $"{createdRooms[index + i].PlayerCount}/{createdRooms[index + i].MaxPlayers}";
                } else {
                    roomPBs[i].gameObject.SetActive(false);
                }

            }
        } else {
            Page.gameObject.SetActive(false);
            BrowserText.gameObject.SetActive(true);
        }
    }

    public void RefreshRoomsList() {
        if (PhotonNetwork.IsConnected) {
            //Re-join Lobby to get the latest Room list
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        } else {
            //We are not connected, estabilish a new connection
            PhotonNetwork.ConnectUsingSettings();
        }
        RefreshRoomPages();
    }

    public void JoinRoom(int index) {
        string playerName = nameInputField.text;
        if (playerName == "") {
            playerName = randomNames[Random.Range(0, randomNames.Count)];
        }
        PhotonNetwork.NickName = playerName;
        PhotonNetwork.JoinRoom(createdRooms[pageNumber * roomsPerPage + index].Name);
    }

    public void NextPage() {
        pageNumber++;
        RefreshRoomPages();
    }

    public void PrevPage() {
        pageNumber--;
        RefreshRoomPages();
    }
}
