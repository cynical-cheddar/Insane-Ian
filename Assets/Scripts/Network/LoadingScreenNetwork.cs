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

    public Text percentText;
    
    private GamestateTracker tracker;

    private int playersInLobby;
    private int playersDone = 0;
    private bool ready = false;

    private int loadedPlayers = 0;
    private int playersInRoom = 0;

    
    void Start()
    {
        playersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        // get the next scene from the gamestate tracker
        tracker = FindObjectOfType<GamestateTracker>();
        nextScene = tracker.nextMap;
        nextSceneDisplayName = tracker.nextMapDisplay;
        nextMapText.text = nextSceneDisplayName;
        // tell all players to load it async
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;

        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC(nameof(StartLoadAsync_RPC), RpcTarget.All);
        }

        // when done, callback to the master, buffered via server

        // when number of finished players = players in room, then load the next scene
    }

    [PunRPC]
    void StartLoadAsync_RPC()
    {
        StartCoroutine((LoadAsync(nextScene)));
    }


    [PunRPC]
    void AddReady()
    {
        loadedPlayers += 1;
    }
    IEnumerator LoadAsync(string name)
    {

        bool sentReady = false;
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        operation.allowSceneActivation = false;

        while (operation.isDone == false)
        {

            float fltProgress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log(fltProgress);
            progressBar.fillAmount = fltProgress;
            if(fltProgress < 0.99) percentText.text = Mathf.RoundToInt(fltProgress * 100) + "%";
            else percentText.text = "Waiting for players";

            if (operation.progress > 0.85 && sentReady == false)
            {
                GetComponent<PhotonView>().RPC(nameof(AddReady), RpcTarget.All);
                sentReady = true;
            }
            if(loadedPlayers >= playersInRoom)
            {
                operation.allowSceneActivation = true;
            }
            yield return null;
            
        }
    }
    
    



    // Update is called once per frame
    void Update()
    {
        progressBar.fillAmount = PhotonNetwork.LevelLoadingProgress;
    }
}
