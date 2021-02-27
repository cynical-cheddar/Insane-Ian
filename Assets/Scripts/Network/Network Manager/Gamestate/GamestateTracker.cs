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
using Gamestate;

public class GamestateTracker : MonoBehaviourPunCallbacks
{
    public List<string> destoryOnTheseLevels = new List<string>();

    public enum Table { Players, Teams, Number }

    private GlobalsEntry globals;
    private List<GamestateTable<GamestateEntry>> tables;




    ScoreboardBehaviour scoreboard;
    public float timeLimit;

    [SerializeField] public PlayerSchema schema = new PlayerSchema();
    [SerializeField] public MapDetails mapDetails = new MapDetails();
    

    [Serializable]
    public struct PlayerSchema
    {
        public List<PlayerDetails> playerList;
        public List<TeamDetails> teamsList;
    }
    
    [Serializable]
    public struct PlayerDetails
    {
        public string nickName;
        public int playerId;
        public string role;
        public int teamId;
        public bool isBot;
        public bool ready;
    }
    
    [Serializable]
    public struct TeamDetails {
        public int teamId, kills, deaths, assists, checkpoint;
        public bool isDead;
        public string vehiclePrefabName;

        public TeamDetails(int id) {
            teamId = id;
            kills = 0;
            deaths = 0;
            assists = 0;
            checkpoint = 0;
            isDead = false;
            vehiclePrefabName = null;
        }

    }

    [Serializable]
    public struct MapDetails
    {
        public string sceneName;
        public string sceneDisplayName;
    }  


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

        globals = new GlobalsEntry();
        tables = new List<GamestateTable<GamestateEntry>>();
        for (int i = 0; i < (int)Table.Number; i++) {
            tables.Add(new GamestateTable<GamestateEntry>());
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {   
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

    public GlobalsEntry GetGlobals() {
        return globals;
    }

    public T GetEntry<T>(Table table, short id) where T : GamestateEntry {
        if (table == Table.Number) throw new Exception("Table.Number cannot be accessed as a table.");

        GamestateTable<GamestateEntry> t = tables[(int)table];
        for (int i = 0; i < t.entries.Count; i++) {
            if (t.entries[i].shortValues[0] == id) return t.entries[i] as T;
        }

        return null;
    }

    public T SearchForEntry<T>(Table table, List<(Enum, short)> shortFieldValues) where T : GamestateEntry {
        if (table == Table.Number) throw new Exception("Table.Number cannot be accessed as a table.");

        GamestateTable<GamestateEntry> t = tables[(int)table];
        for (int i = 0; i < t.entries.Count; i++) {
            bool found = true;

            for (int j = 0; j < shortFieldValues.Count; j++) {
                int fieldIndex = Convert.ToInt32(shortFieldValues[j].Item1);
                short fieldValue = shortFieldValues[j].Item2;

                if (t.entries[i].shortValues[fieldIndex] != fieldValue) {
                    found = false;
                    break;
                }
            }

            if (found) return t.entries[i] as T;
        }

        return null;
    }



    // returns a photonPlayer by looking up allplayers in room
    public Player GetPlayerFromDetails(PlayerDetails pd)
    {
        return PhotonNetwork.LocalPlayer;
    }


    public PlayerDetails GenerateDefaultPlayerDetails(string nickName, int playerId)
    {
        return new PlayerDetails();
    }

    public PlayerDetails generateBotDetails()
    {
        return new PlayerDetails();
    }

    public int GetNumberOfBotsInGame()
    {
        return 0;
    }
    public PlayerDetails generateBotDetails(string nickName)
    {
        return new PlayerDetails();
    }
    
    
    public PlayerDetails getPlayerDetails(int id) {
        return new PlayerDetails();
    }

    public TeamDetails getTeamDetails(int teamId) {
        return new TeamDetails();
    }
    
    [PunRPC]
    public void RemovePlayerFromSchema(int id)
    {
        
    }

    // gets the player list of the master client and forces synchronisation
    // this is network costly so we do not buffer it.
    // may deprecate older ways of doing things, such as RPC calls to gamestate tracker
    // Called to double check that everyone is synchronised correctly
    public void ForceSynchronisePlayerSchema()
    {
        
    }

    [PunRPC]    
    public void AddBotToSchema(string serialisedPlayerDetails)
    {
        
    }

    public bool mayAddBotToSchema(PlayerDetails bd)
    {
        return true;
    }
    
    // preferred method
    [PunRPC]
    public void UpdatePlayerWithNewRecord(int id, string newDetailsSerialized)
    {
        
    }

    // call this via rpc
    [PunRPC]
    public void UpdateTeamWithNewRecord(int teamId, string newDetailsSerialized) {
        
    }

    public void UpdateTeamWithNewRecord(int teamId, TeamDetails newDetails) {
        
    }

    // return a list of pairs in the playerlist
    public List<List<PlayerDetails>> GetPlayerPairs()
    {
        return new List<List<PlayerDetails>>();
    }

    public PlayerDetails GetPlayerWithDetails(string nickname = null, int playerId = 0, string role = null, string character = null, int teamId = 0)
    {
        return new PlayerDetails();
    }
}
