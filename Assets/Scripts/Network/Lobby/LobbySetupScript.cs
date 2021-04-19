using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;


public class LobbySetupScript : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    
    void Start()
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
