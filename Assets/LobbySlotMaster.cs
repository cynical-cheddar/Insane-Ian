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
    public PlayerSchema playerSlotSchema;


    // Start is called before the first frame update
    void Start()
    {
        maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        playersInLobby = PhotonNetwork.CurrentRoom.PlayerCount;
        AddPlayerToSchema(PhotonNetwork.LocalPlayer);

        if(PhotonNetwork.IsMasterClient)GetComponent<PhotonView>().RPC("refreshButtons", RpcTarget.All, maxPlayers);
        
    }
    
    public void AddPlayerToSchema(Player p)
    {
        playerSlotSchema.schema.Add((p, p.NickName, "null", "null", 0));
    }

    public void RemovePlayerFromSchema(Player p)
    {
        foreach ((Player, string, string, string, int) record in playerSlotSchema.schema)
        {
            if (record.Item1.Equals(p))
            {
                playerSlotSchema.schema.Remove((record));
            }
        }
    }

    public void UpdatePlayerInSchema(Player p, string role, string character, int team)
    {
        foreach ((Player, string, string, string, int) record in playerSlotSchema.schema)
        {
            (Player, string, string, string, int) newRecord;
            bool found = false;
            if (record.Item1.Equals(p))
            {
                found = true;
                newRecord = (record.Item1, record.Item2, role, character, team);
                playerSlotSchema.schema.Add(newRecord);
            }
            if (found)
            {
                playerSlotSchema.schema.Remove(record);
                
            }
            
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(PhotonNetwork.IsMasterClient)GetComponent<PhotonView>().RPC("refreshButtons", RpcTarget.All, maxPlayers, playerSlotSchema);
    }

    // create a copy of the menu that the host has on their instance of the game absed on the host schema
    [PunRPC]
    public void refreshButtons(int maxPlayers, List<(Player, string, string, string, int)> hostSchema)
    {
        foreach (Transform child in slotMaster)
        {
            Destroy(child);
        }
        // for now, instantiate buttons for maxPlayers / 2
        for(int i = 0; i< maxPlayers / 2; i++)
        {
           GameObject a = Instantiate(buttonsPrefab, slotMaster);
           // set lobby button stuff
           LobbyButtonScript lobbyButtonScript = a.GetComponent<LobbyButtonScript>();
           
           // see if there are any players belonging to team i, fill in their buttons
           List<(Player, string, string, string, int)> playersInTeam = GetPlayersFromHostSchemaByTeam(hostSchema, i);
           // there are players in this team. Update the buttons as per the schema
           if (playersInTeam.Count > 0)
           {
               lobbyButtonScript.setButtonInfo(playersInTeam);
           }
        }
    }

    List<(Player, string, string, string, int)> GetPlayersFromHostSchemaByTeam(List<(Player, string, string, string, int)> hostSchema, int teamId)
    {
        List<(Player, string, string, string, int)> playerList = new List<(Player, string, string, string, int)>();
        foreach ((Player, string, string, string, int) record in hostSchema)
        {
            if (record.Item5 == teamId)
            {
                playerList.Add(record);
            }
        }

        return playerList;
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
