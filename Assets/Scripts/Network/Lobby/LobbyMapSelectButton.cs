using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Gamestate;

public class LobbyMapSelectButton : MonoBehaviour
{
    public bool uninteractableIfNotMaster = true;
    public string sceneName = "SampleScene";
    public string sceneDisplayName = "Testing Arena";
    public Image mapIconUI;
    public Sprite mapIcon;
    public Text mapName;
    public LobbySlotMaster lobbySlotMaster;
    private GamestateTracker _gamestateTracker;
    void Start()
    {
        _gamestateTracker = FindObjectOfType<GamestateTracker>();
        if (uninteractableIfNotMaster && !PhotonNetwork.IsMasterClient)
        {
            GetComponent<Button>().interactable = false;
        }
        
    }

    public void SelectMap()
    {
        lobbySlotMaster.selectedMap = sceneName;
        GetComponent<PhotonView>().RPC(nameof(UpdateMapImageAndName), RpcTarget.AllBufferedViaServer);
        _gamestateTracker.GetComponent<PhotonView>().RPC(nameof(GamestateTracker.UpdateMapDetails), RpcTarget.AllBufferedViaServer, sceneName, sceneDisplayName);

    }

    [PunRPC]
    public void UpdateMapImageAndName()
    {
        mapIconUI.sprite = mapIcon;
        mapName.text = sceneDisplayName;
    }
}
