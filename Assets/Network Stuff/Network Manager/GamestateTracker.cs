using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamestateTracker : MonoBehaviour
{
    // Start is called before the first frame update
    public List<string> destoryOnTheseLevels = new List<string>();
    public int maxPlayers = 24;

    [SerializeField]
    public List<PlayerDetails> playerList = new List<PlayerDetails>();
    [SerializeField]
    public MapDetails mapDetails = new MapDetails();
    
    
    [Serializable]
    public struct PlayerDetails
    {
        public Player player;
        public string nickName;
        public string role;
        public string character;
        public int teamId;
        public PlayerDetails(Player p, string n, string r, string c, int t)
        {
            player = p; nickName = n; role = r; character = c; teamId = t;}
    }
    
    [Serializable]
    public struct MapDetails
    {
        public string sceneName;
        public string sceneDisplayName;
        public MapDetails(string sn, string sdn)
        {
            sceneName = sn; sceneDisplayName = sdn;}
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name + " time to destroy the gametracker");
        
        if (destoryOnTheseLevels.Contains(scene.name))
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    
    
    
    
    
    
    public PlayerDetails w(Player p)
    {

        PlayerDetails pd = new PlayerDetails();
        foreach (PlayerDetails record in playerList)
        {
            if (record.player.Equals(p))
            {
                return record;
            }
        }

        return pd;
    }

    [PunRPC]
    public void UpdateMapDetails(string newSceneName, string newSceneDisplayName)
    {
        mapDetails.sceneName = newSceneName;
        mapDetails.sceneDisplayName = newSceneDisplayName;
    }
    [PunRPC]
    public void UpdateSceneName(string sceneName)
    {
        mapDetails.sceneName = sceneName;
    }
    [PunRPC]
    public void UpdateMapDisplayName(string displayName)
    {
        mapDetails.sceneDisplayName = displayName;
    }
    
    
    
    [PunRPC]
    public void RemovePlayerFromSchema(Player p)
    {
        foreach (PlayerDetails record in playerList)
        {
            if (record.player.Equals(p))
            {
                playerList.Remove(record);
            }
        }
    }
    [PunRPC]    
    public void AddFirstPlayerToSchema(Player p)
    {

        PlayerDetails pd = new PlayerDetails(p, p.NickName, "null", "null", 0);


        playerList[0] = pd;
    }
    [PunRPC]    
    public void AddPlayerToSchema(Player p)
    {

        PlayerDetails pd = new PlayerDetails(p, p.NickName, "null", "null", 0);
        
            
        playerList.Add(pd);
    }
    [PunRPC]
    public void AddPlayerToSchema(PlayerDetails pd)
    {
        playerList.Add(pd);
    }
    
    // preferred method
    [PunRPC]
    public void UpdatePlayerWithNewRecord(Player p, PlayerDetails newDetails)
    {
        bool found = false;
        PlayerDetails oldRecord= playerList[0];
        foreach (PlayerDetails record in playerList)
        {
            if (record.player.Equals(p))
            {
                found = true;
                oldRecord = record;

            }
        }
        if (found)
        {
            playerList.Remove(oldRecord);
            playerList.Add(newDetails);
        }
    }
    [PunRPC]
    public void UpdatePlayerRole(Player p, string role)
    {
        bool found = false;
        PlayerDetails oldRecord= playerList[0];
        PlayerDetails newRecord = playerList[0];
        foreach (PlayerDetails record in playerList)
        {
            if (record.player.Equals(p))
            {
                found = true;
                oldRecord = record;
                newRecord = record;
                newRecord.role = role;
                
            }
        }
        if (found)
        {
            playerList.Remove(oldRecord);
            playerList.Add(newRecord);
        }
    }
    [PunRPC]
    public void UpdatePlayerCharacter(Player p, string character)
    {
        bool found = false;
        PlayerDetails oldRecord= playerList[0];
        PlayerDetails newRecord = playerList[0];
        foreach (PlayerDetails record in playerList)
        {
            if (record.player.Equals(p))
            {
                found = true;
                oldRecord = record;
                newRecord = record;
                newRecord.character = character;
                
            }
        }
        if (found)
        {
            playerList.Remove(oldRecord);
            playerList.Add(newRecord);
        }
    }
    [PunRPC]
    public void UpdatePlayerTeam(Player p, int team)
    {
        bool found = false;
        PlayerDetails oldRecord= playerList[0];
        PlayerDetails newRecord = playerList[0];
        foreach (PlayerDetails record in playerList)
        {
            if (record.player.Equals(p))
            {
                found = true;
                oldRecord = record;
                newRecord = record;
                newRecord.teamId = team;
                
            }
        }
        if (found)
        {
            playerList.Remove(oldRecord);
            playerList.Add(newRecord);
        }
    }

        
    
    
    
    
    
    
    
    
    

        
        
        
}
