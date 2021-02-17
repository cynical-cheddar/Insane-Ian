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

public class GamestateTracker : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public List<string> destoryOnTheseLevels = new List<string>();
    public int maxPlayers = 24;
    ScoreboardBehaviour scoreboard;

    [SerializeField] public PlayerSchema schema = new PlayerSchema();
    [SerializeField] public MapDetails mapDetails = new MapDetails();
    

    [Serializable]
    public struct PlayerSchema
    {
        public List<PlayerDetails> playerList;
        public List<TeamDetails> teamsList;

        public PlayerSchema(List<PlayerDetails> pdl, List<TeamDetails> tdl)
        {
            playerList = pdl;
            teamsList = tdl;
        }
    }
    
    [Serializable]
    public struct PlayerDetails
    {
        public string nickName;
        public int playerId;
        public string role;
        public string character;
        public int teamId;
        public bool isBot;
        
        
        public PlayerDetails(string n, int p, string r, string c, int t, bool b, string v)
        {
            nickName = n;
            playerId = p;
            role = r;
            character = c;
            teamId = t;
            isBot = b;
            
        }
    }
    
    [Serializable]
    public struct TeamDetails {
        public int teamId, kills, deaths, assists;
        public bool isDead;
        public string vehiclePrefabName;

        public TeamDetails(int id) {
            teamId = id;
            kills = 0;
            deaths = 0;
            assists = 0;
            isDead = false;
            vehiclePrefabName = null;
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
        schema = new PlayerSchema(new List<PlayerDetails>(), new List<TeamDetails>());
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

        if (PhotonNetwork.IsMasterClient)
        {
            PlayerDetails firstPd = GenerateDefaultPlayerDetails("null", PhotonNetwork.LocalPlayer.ActorNumber);
            string pdJson = JsonUtility.ToJson(firstPd);
            GetComponent<PhotonView>().RPC(nameof(AddPlayerToSchema), RpcTarget.AllViaServer, pdJson);
            
        }
    }

    // returns a photonPlayer by looking up allplayers in room
    public Player GetPlayerFromDetails(PlayerDetails pd)
    {
        return PhotonNetwork.CurrentRoom.GetPlayer(pd.playerId);
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("OnSceneLoaded: " + scene.name + " time to destroy the gametracker");
        
        if (destoryOnTheseLevels.Contains(scene.name))
        {
            PhotonNetwork.Destroy(gameObject);
        }

        if (FindObjectOfType<ScoreboardBehaviour>() != null) {
            scoreboard = FindObjectOfType<ScoreboardBehaviour>();
        } else {
            scoreboard = null;
        }
    }

    public PlayerDetails GenerateDefaultPlayerDetails(string nickName, int playerId)
    {
        PlayerDetails bd = new PlayerDetails();
        bd.nickName = nickName;
        bd.playerId = playerId;
        bd.character = "null";
        bd.role = "null";
        bd.teamId = 0;
        bd.isBot = false;
        return bd;
    }

    // generates a bot with generic name
    public PlayerDetails generateBotDetails()
    {
        PlayerDetails bd = new PlayerDetails();
        
        // get the number of bots in the game
        bd.playerId = -1;
        for (int i = 0; i < schema.playerList.Count; i++) {
            if (schema.playerList[i].isBot && bd.playerId == schema.playerList[i].playerId) {
                bd.playerId--;
                i = 0;
            }
        }

        bd.nickName = "Bot " + bd.playerId;

        bd.character = "null";
        bd.role = "null";
        bd.teamId = 0;
        bd.isBot = true;
        
        return bd;
    }

    public int GetNumberOfBotsInGame()
    {
        int botCount = 0;
        foreach (PlayerDetails pd in schema.playerList)
        {
            if (pd.isBot) botCount++;
        }

        return botCount;
    }
    public PlayerDetails generateBotDetails(string nickName)
    {
        PlayerDetails bd = generateBotDetails();
        bd.nickName = nickName;
        
        return bd;
    }
    
    
    public PlayerDetails getPlayerDetails(int id)
    {

        PlayerDetails pd = new PlayerDetails();
        foreach (PlayerDetails record in schema.playerList)
        {
            if (record.playerId.Equals(id))
            {
                return record;
            }
        }

        return pd;
    }

    public TeamDetails getTeamDetails(int teamId) {
        TeamDetails td = new TeamDetails();
        foreach (TeamDetails record in schema.teamsList) {
            if (record.teamId.Equals(teamId)) {
                return record;
            }
        }
        
        return td;
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
    public void RemovePlayerFromSchema(int id)
    {
        PlayerDetails recordToRemove = new PlayerDetails();
        bool found = false;
        foreach (PlayerDetails record in schema.playerList)
        {
            if (record.playerId == id)
            {
                found = true;
                recordToRemove = record;
            }
        }

        if(found)schema.playerList.Remove(recordToRemove);
    }
    [PunRPC]    
    public void AddFirstPlayerToSchema(string serializedPlayerDetails)
    {
        PlayerDetails pd = JsonUtility.FromJson<PlayerDetails>(serializedPlayerDetails);
        //Debug.Log(serializedPlayerDetails);
        //Debug.Log(pd);
        schema.playerList[0] = pd;
    }

    // gets the player list of the master client and forces synchronisation
    // this is network costly so we do not buffer it.
    // may deprecate older ways of doing things, such as RPC calls to gamestate tracker
    // Called to double check that everyone is synchronised correctly
    public void ForceSynchronisePlayerSchema()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // get the whole schema and convert it to a json
            string schemaJson = JsonUtility.ToJson(schema);
            //Debug.Log("playerListJSON = " + schemaJson);
            GetComponent<PhotonView>().RPC(nameof(UpdatePlayerListFromMasterClient), RpcTarget.All, schemaJson);
        }
    }

    // received by non master clients. Updates player list to the true version

    [PunRPC]
    void UpdatePlayerListFromMasterClient(string playerSchemaJSON)
    {
        //Debug.Log("playerListJSON = " + playerSchemaJSON);
        PlayerSchema newPlayerSchema = JsonUtility.FromJson<PlayerSchema>(playerSchemaJSON);
        schema = newPlayerSchema;
    }
    [PunRPC]    
    public void AddBotToSchema(string serialisedPlayerDetails)
    {
        PlayerDetails pd = JsonUtility.FromJson<PlayerDetails>(serialisedPlayerDetails);
        schema.playerList.Add(pd);
        ForceSynchronisePlayerSchema();
    }
    
    [PunRPC]
    public void AddPlayerToSchema(string serialisedPlayerDetails)
    {
        
        PlayerDetails pd = JsonUtility.FromJson<PlayerDetails>(serialisedPlayerDetails);
        //Debug.Log("adding player to schema: " + pd.nickName + " " + pd.playerId + " " + pd.role + " " + pd.teamId.ToString() + " bot status: " + pd.isBot.ToString());
        schema.playerList.Add(pd);
        ForceSynchronisePlayerSchema();
    }

    public bool mayAddBotToSchema(PlayerDetails bd)
    {
        bool passed = true;
        // check for no duplicate names
        // check for nobody in the same slot
        
        // if this is cool and good, then return true
        foreach (PlayerDetails pd in schema.playerList)
        {
            if (pd.playerId == bd.playerId) passed = false;
            if (pd.role == bd.role && pd.teamId == bd.teamId) passed = false;
        }


        return passed;
    }
    [PunRPC]
    public void AddBotToSchema(PlayerDetails pd)
    {
        
        schema.playerList.Add(pd);
    }
    
    // preferred method
    [PunRPC]
    public void UpdatePlayerWithNewRecord(int id, string newDetailsSerialized)
    {
        PlayerDetails newRecord = JsonUtility.FromJson<PlayerDetails>(newDetailsSerialized);
        bool found = false;
        PlayerDetails oldRecord = schema.playerList[0];
        foreach (PlayerDetails record in schema.playerList)
        {
            if (record.playerId == id)
            {
                found = true;
                oldRecord = record;

            }
        }
        if (found)
        {
            schema.playerList.Remove(oldRecord);
            schema.playerList.Add(newRecord);
        }
        ForceSynchronisePlayerSchema();
    }

    // call this via rpc
    [PunRPC]
    public void UpdateTeamWithNewRecord(int teamId, string newDetailsSerialized) {
        TeamDetails newRecord = JsonUtility.FromJson<TeamDetails>(newDetailsSerialized);
        bool found = false;
        TeamDetails oldRecord = schema.teamsList[0];
        foreach (TeamDetails record in schema.teamsList) {
            if (record.teamId.Equals(teamId)) {
                found = true;
                oldRecord = record;
            }
        }
        if (found) {
            schema.teamsList.Remove(oldRecord);
            schema.teamsList.Add(newRecord);
        }
        if (scoreboard != null) {
            scoreboard.UpdateScores();
        }
        ForceSynchronisePlayerSchema();
    }

    public void UpdateTeamWithNewRecord(int teamId, TeamDetails newDetails) {
        bool found = false;
        TeamDetails oldRecord = schema.teamsList[0];
        foreach (TeamDetails record in schema.teamsList) {
            if (record.teamId.Equals(teamId)) {
                found = true;
                oldRecord = record;
            }
        }
        if (found) {
            schema.teamsList.Remove(oldRecord);
            schema.teamsList.Add(newDetails);
        }
        if (scoreboard != null) {
            scoreboard.UpdateScores();
        }
        ForceSynchronisePlayerSchema();
    }

    // JORDAN WILL DEPRECATE EVENTUALLY, DO NOT USE
    [PunRPC]
    public void UpdatePlayerRole(int id, string role)
    {
        bool found = false;
        PlayerDetails oldRecord= schema.playerList[0];
        PlayerDetails newRecord = schema.playerList[0];
        foreach (PlayerDetails record in schema.playerList)
        {
            if (record.playerId == id)
            {
                found = true;
                oldRecord = record;
                newRecord = record;
                newRecord.role = role;
                
            }
        }
        if (found)
        {
            schema.playerList.Remove(oldRecord);
            schema.playerList.Add(newRecord);
        }
        ForceSynchronisePlayerSchema();
    }
    // JORDAN WILL DEPRECATE EVENTUALLY, DO NOT USE
    [PunRPC]
    public void UpdatePlayerCharacter(int id, string character)
    {
        bool found = false;
        PlayerDetails oldRecord= schema.playerList[0];
        PlayerDetails newRecord = schema.playerList[0];
        foreach (PlayerDetails record in schema.playerList)
        {
            if (record.playerId == id)
            {
                found = true;
                oldRecord = record;
                newRecord = record;
                newRecord.character = character;
                
            }
        }
        if (found)
        {
            schema.playerList.Remove(oldRecord);
            schema.playerList.Add(newRecord);
        }
        ForceSynchronisePlayerSchema();
    }
    // JORDAN WILL DEPRECATE EVENTUALLY, DO NOT USE
    [PunRPC]
    public void UpdatePlayerTeam(int id, int team)
    {
        bool found = false;
        PlayerDetails oldRecord= schema.playerList[0];
        PlayerDetails newRecord = schema.playerList[0];
        foreach (PlayerDetails record in schema.playerList)
        {
            if (record.playerId == id)
            {
                found = true;
                oldRecord = record;
                newRecord = record;
                newRecord.teamId = team;
                
            }
        }
        if (found)
        {
            schema.playerList.Remove(oldRecord);
            schema.playerList.Add(newRecord);
        }
        ForceSynchronisePlayerSchema();
    }


    // return a list of pairs in the playerlist
    public List<List<PlayerDetails>> GetPlayerPairs()
    {
        List<GamestateTracker.PlayerDetails> playerDetailsList = schema.playerList;
        List<int> uniqueTeamIds = new List<int>();
        // iterate through list to get all of the team ids
        foreach (GamestateTracker.PlayerDetails record in playerDetailsList)
        {
            if (!uniqueTeamIds.Contains(record.teamId))
            {
                uniqueTeamIds.Add(record.teamId);
            }
        }

        // get a gunner and driver from each unique team, compiling a list of player detail pairs
        List<List<GamestateTracker.PlayerDetails>> playerDetailsPairs = new List<List<GamestateTracker.PlayerDetails>>();
        foreach (int team in uniqueTeamIds)
        {
            // search our current team for players belonging to team i
            List<GamestateTracker.PlayerDetails> pair = new List<GamestateTracker.PlayerDetails>();
            foreach (GamestateTracker.PlayerDetails record in schema.playerList)
            {
                if (record.teamId == team)
                {
                    pair.Add(record);
                }
            }
            // avoid adding the null pair (it shouldn't exist, but it might)
            if (pair.Count > 0)
            {
                playerDetailsPairs.Add(pair);
            }
        }

        return playerDetailsPairs;
    }

    public PlayerDetails GetPlayerWithDetails(string nickname = null, int playerId = 0, string role = null, string character = null, int teamId = 0)
    {
        foreach (GamestateTracker.PlayerDetails record in schema.playerList)
        {
            if ((nickname  == null || record.nickName  == nickname)  &&
                (playerId  == 0    || record.playerId  == playerId)  &&
                (role      == null || record.role      == role)      &&
                (character == null || record.character == character) &&
                (teamId    == 0    || record.teamId    == teamId))
            {
                return record;
            }
        }

        return new PlayerDetails();
    }

    
    
    
    
    
    
    
    
    

        
        
        
}
