using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Gamestate;
public class LoadingScreenNetwork : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    private string nextScene;
    string nextSceneDisplayName;

    public Text nextMapText;

    public Image progressBar;

    private GamestateTracker tracker;

    private int playersInLobby;
    private int playersDone = 0;
    private bool ready = false;
    void Start()
    {
        // get the next scene from the gamestate tracker
        tracker = FindObjectOfType<GamestateTracker>();
        nextScene = tracker.mapDetails.sceneName;
        nextSceneDisplayName = tracker.mapDetails.sceneDisplayName;
        nextMapText.text = nextSceneDisplayName;
        // tell all players to load it async
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(nextScene);
        }

        // when done, callback to the master, buffered via server

        // when number of finished players = players in room, then load the next scene
    }
    
    



    // Update is called once per frame
    void Update()
    {
        progressBar.fillAmount = PhotonNetwork.LevelLoadingProgress;
    }
}
