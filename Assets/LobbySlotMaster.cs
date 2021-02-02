using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbySlotMaster : MonoBehaviourPunCallbacks
{

    private int readyPlayers = 0;

    private int selectedPlayers = 0;

    private int playersInLobby = 1;

    private int maxPlayers = 0;

    public GameObject buttonsPrefab;

    public Transform slotMaster;

    // player, nickname, role, character
    public PlayerSchema playerSlotSchema = new PlayerSchema();


    // Start is called before the first frame update
    void Start()
    {
        maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        
        // add ourselves
        playerSlotSchema = playerSlotSchema.AddPlayerToSchema(PhotonNetwork.LocalPlayer, playerSlotSchema);

        if(PhotonNetwork.IsMasterClient)GetComponent<PhotonView>().RPC("refreshButtons", RpcTarget.All, maxPlayers);
        
    }
    




    

    // Update is called once per frame
    void FixedUpdate()
    {
        if(PhotonNetwork.IsMasterClient)GetComponent<PhotonView>().RPC("refreshButtons", RpcTarget.All, maxPlayers, playerSlotSchema);
    }

    // create a copy of the menu that the host has on their instance of the game absed on the host schema
    [PunRPC]
    public void refreshButtons(int maxPlayers, PlayerSchema hostSchema)
    {
        // remove previous buttons
        foreach (Transform child in slotMaster)
        {
            Destroy(child);
        }
        // for now, instantiate buttons for maxPlayers / 2
        for(int i = 0; i< maxPlayers / 2; i++)
        {
            // instantiate button with slotMaster as parent
           GameObject a = Instantiate(buttonsPrefab, slotMaster);
           // set lobby button stuff
           LobbyButtonScript lobbyButtonScript = a.GetComponent<LobbyButtonScript>();
           
           // see if there are any players belonging to team i, fill in their buttons
           PlayerSchema playersInTeam = hostSchema.GetPlayersFromHostSchemaByTeam(hostSchema, i);
           
           // there are players in this team. Update the buttons as per the schema
           if (playersInTeam.schema.Count > 0)
           {
               lobbyButtonScript.setButtonInfo(playersInTeam);
           }
        }
    }

    

    [PunRPC]
    public void selectRolePlayer()
    {
        
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        
    }
}
