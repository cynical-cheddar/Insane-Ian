using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Gamestate;
using TMPro;

public class ReadyToggle : MonoBehaviour
{

    public LobbySlotMaster lobbySlotMaster;
    GamestateTracker gamestateTracker;
    bool isReady = false;
    public Button toggleReadyButton;
    public Image buttonFill;
    public TextMeshProUGUI buttonText;
    // Start is called before the first frame update
    void Start()
    {
        if (lobbySlotMaster == null)
        {
            lobbySlotMaster = FindObjectOfType<LobbySlotMaster>();
        }

        gamestateTracker = FindObjectOfType<GamestateTracker>();
    }

    public void ToggleReady() {
        SetReady(!isReady);
    }

    void SetReady(bool state) {
        isReady = state;
        PlayerEntry playerEntry = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);

        if (playerEntry.role == (short)PlayerEntry.Role.None) isReady = false;

        lobbySlotMaster.GetComponent<PhotonView>().RPC(nameof(LobbySlotMaster.UpdateCountAndReady), RpcTarget.All);
        playerEntry.ready = isReady;
        playerEntry.Commit();

        if (!isReady) {
            buttonFill.color = new Color32(0xFF, 0x61, 0x61, 0xFF);
            buttonText.text = "Ready Up";
        } else {
            buttonFill.color = new Color32(0x65, 0xC5, 0x6B, 0xFF);
            buttonText.text = "Unready";
        }
    }

}
