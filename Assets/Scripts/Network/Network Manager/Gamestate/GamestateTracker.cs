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

namespace Gamestate {
    public class GamestateTracker : MonoBehaviourPunCallbacks, IGamestateCommitHandler
    {
        public List<string> destoryOnTheseLevels = new List<string>();

        public enum Table { Globals, Players, Teams }

        public int actorNumber {
            get { return PhotonNetwork.LocalPlayer.ActorNumber; }
        }

        public Table tableType { get { return Table.Globals; } }

        private GlobalsEntry _globals;
        public GlobalsEntry globals {
            get {
                _globals.Lock();
                return _globals;
            }
        }

        private GamestateTable<PlayerEntry> _players;
        public GamestateTable<PlayerEntry> players { get { return _players; } }

        private GamestateTable<TeamEntry> _teams;
        public GamestateTable<TeamEntry> teams { get { return _teams; } }

        private PhotonView view;




        //  START DEPRECATED
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
        //  END DEPRECATED



        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            view = GetComponent<PhotonView>();

            _globals = new GlobalsEntry(this);
            _players = new GamestateTable<PlayerEntry>(this, Table.Players);
            _teams = new GamestateTable<TeamEntry>(this, Table.Teams);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {   
            if (destoryOnTheseLevels.Contains(scene.name))
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        void IGamestateCommitHandler.CommitPacket(GamestatePacket packet) {
            if (PhotonNetwork.LocalPlayer.IsMasterClient) {
                ApplyPacket(packet);
            }
            else {
                view.RPC(nameof(ApplyPacket), RpcTarget.MasterClient, packet);
            }
        }

        [PunRPC]
        public void ApplyPacket(GamestatePacket packet) {
            if (PhotonNetwork.LocalPlayer.IsMasterClient) {
                bool succeeded = false;

                if (packet.table == Table.Globals) {
                    if (packet.packetType != GamestatePacket.PacketType.Delete) {
                        succeeded = _globals.AttemptApply(packet);
                    }
                }
                else if (packet.table == Table.Players) {
                    succeeded = players.AttemptApply(packet);
                }
                else if (packet.table == Table.Teams) {
                    succeeded = teams.AttemptApply(packet);
                }

                if (succeeded) view.RPC(nameof(ApplyPacket), RpcTarget.Others, packet);
            }
            else {
                if (packet.table == Table.Globals) {
                    if (packet.packetType != GamestatePacket.PacketType.Delete) {
                        _globals.Apply(packet);
                    }
                }
                else if (packet.table == Table.Players) {
                    players.Apply(packet);
                }
                else if (packet.table == Table.Teams) {
                    teams.Apply(packet);
                }
            }
        }




        //  START DEPRECATED
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
}
