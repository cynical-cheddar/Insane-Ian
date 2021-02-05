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
        public string nickName;
        public string role;
        public string character;
        public int teamId;
        public bool isBot;
        public PlayerDetails(string n, string r, string c, int t, bool b)
        {
            nickName = n; role = r; character = c; teamId = t;
            isBot = b;
        }
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

    public PlayerDetails GenerateDefaultPlayerDetails(string nickName)
    {
        PlayerDetails bd = new PlayerDetails();
        bd.nickName = nickName;
        bd.character = "null";
        bd.role = "null";
        bd.teamId = 0;
        return bd;
    }

    // generates a bot with generic name
    public PlayerDetails generateBotDetails()
    {
        PlayerDetails bd = new PlayerDetails();
        
        // get the number of bots in the game
        int botCount = 0;
        foreach (PlayerDetails pd in playerList)
        {
            if (pd.isBot) botCount++;
        }

        bd.nickName = "Bot " + botCount.ToString();

        bd.character = "null";
        bd.role = "null";
        bd.teamId = 0;
        bd.isBot = true;
        
        return bd;
    }
    public PlayerDetails generateBotDetails(string nickName)
    {
        PlayerDetails bd = new PlayerDetails();
        
        // get the number of bots in the game
        int botCount = 0;
        foreach (PlayerDetails pd in playerList)
        {
            if (pd.nickName == nickName) botCount++;
        }

        bd.nickName = nickName + botCount.ToString();

        bd.character = "null";
        bd.role = "null";
        bd.teamId = 0;
        
        return bd;
    }
    
    
    public PlayerDetails getPlayerDetails(string p)
    {

        PlayerDetails pd = new PlayerDetails();
        foreach (PlayerDetails record in playerList)
        {
            if (record.nickName.Equals(p))
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
    public void RemovePlayerFromSchema(string p)
    {
        PlayerDetails recordToRemove = new PlayerDetails();
        bool found = false;
        foreach (PlayerDetails record in playerList)
        {
            if (record.nickName.Equals(p))
            {
                found = true;
                recordToRemove = record;
            }
        }

        if(found)playerList.Remove(recordToRemove);
    }
    [PunRPC]    
    public void AddFirstPlayerToSchema(string serializedPlayerDetails)
    {
        PlayerDetails pd = JsonUtility.FromJson<PlayerDetails>(serializedPlayerDetails);
        playerList[0] = pd;
    }

    // gets the player list of the master client and forces synchronisation
    // this is network costly so we do not buffer it.
    // may deprecate older ways of doing things, such as RPC calls to gamestate tracker
    public void ForceSynchronisePlayerList()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // get the whole schema and convert it to a json
            string playerListJSON = JsonUtility.ToJson(playerList);
            GetComponent<PhotonView>().RPC("UpdatePlayerListFromMasterClient", RpcTarget.Others, playerListJSON);
        }
    }
    
    // received by non master clients. Updates player list to the true version

    [PunRPC]
    void UpdatePlayerListFromMasterClient(string playerListJSON)
    {
        List<PlayerDetails> newPlayerList = JsonUtility.FromJson<List<PlayerDetails>>(playerListJSON);
        playerList = newPlayerList;
    }
    [PunRPC]    
    public void AddBotToSchema(string serialisedPlayerDetails)
    {
        PlayerDetails pd = JsonUtility.FromJson<PlayerDetails>(serialisedPlayerDetails);
        playerList.Add(pd);
        ForceSynchronisePlayerList();
    }
    
    [PunRPC]
    public void AddPlayerToSchema(string serialisedPlayerDetails)
    {
        
        PlayerDetails pd = JsonUtility.FromJson<PlayerDetails>(serialisedPlayerDetails);
        Debug.Log("adding player to schema: " + pd.nickName + " " + pd.role + " " + pd.teamId.ToString() + " bot status: " + pd.isBot.ToString());
        playerList.Add(pd);
        ForceSynchronisePlayerList();
    }

    public bool mayAddBotToSchema(PlayerDetails bd)
    {
        bool passed = true;
        // check for no duplicate names
        // check for nobody in the same slot
        
        // if this is cool and good, then return true
        foreach (PlayerDetails pd in playerList)
        {
            if (pd.nickName == bd.nickName) passed = false;
            if (pd.role == bd.role && pd.teamId == bd.teamId) passed = false;
        }


        return passed;
    }
    [PunRPC]
    public void AddBotToSchema(PlayerDetails pd)
    {
        
        playerList.Add(pd);
    }
    
    // preferred method
    [PunRPC]
    public void UpdatePlayerWithNewRecord(string p, PlayerDetails newDetails)
    {
        bool found = false;
        PlayerDetails oldRecord= playerList[0];
        foreach (PlayerDetails record in playerList)
        {
            if (record.nickName.Equals(p))
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
        ForceSynchronisePlayerList();
    }
    [PunRPC]
    public void UpdatePlayerRole(string p, string role)
    {
        bool found = false;
        PlayerDetails oldRecord= playerList[0];
        PlayerDetails newRecord = playerList[0];
        foreach (PlayerDetails record in playerList)
        {
            if (record.nickName.Equals(p))
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
        ForceSynchronisePlayerList();
    }
    [PunRPC]
    public void UpdatePlayerCharacter(string p, string character)
    {
        bool found = false;
        PlayerDetails oldRecord= playerList[0];
        PlayerDetails newRecord = playerList[0];
        foreach (PlayerDetails record in playerList)
        {
            if (record.nickName.Equals(p))
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
        ForceSynchronisePlayerList();
    }
    [PunRPC]
    public void UpdatePlayerTeam(string p, int team)
    {
        bool found = false;
        PlayerDetails oldRecord= playerList[0];
        PlayerDetails newRecord = playerList[0];
        foreach (PlayerDetails record in playerList)
        {
            if (record.nickName.Equals(p))
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
        ForceSynchronisePlayerList();
    }

        
    
    
    
    
    
    
    
    
    

        
        
        
}
