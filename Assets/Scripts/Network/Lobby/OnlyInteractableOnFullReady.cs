using System.Collections;
using System.Collections.Generic;
using Gamestate;
using UnityEngine;
using UnityEngine.UI;
public class OnlyInteractableOnFullReady : MonoBehaviour
{
    private GamestateTrackerUtils gamestateTrackerUtils;

    private Button btn;
    LobbySlotMaster lsm;

    // Start is called before the first frame update
    void Start()
    {
        gamestateTrackerUtils = FindObjectOfType<GamestateTrackerUtils>();
        btn = GetComponent<Button>();
        lsm = FindObjectOfType<LobbySlotMaster>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lsm.readyPlayers >= lsm.selectedPlayers && lsm.readyPlayers >= lsm.playersInLobby && lsm.selectedMap != "null") btn.interactable = true;
        else btn.interactable = false;
    }
}
