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
        protected bool bufferPackets = false;
        public List<string> bufferRpcScenes;

        //  FUCK
        public string nextMap;
        public string nextMapDisplay;
        
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

        [SerializeField] private GamestateTable<PlayerEntry> _players;
        public GamestateTable<PlayerEntry> players { get { return _players; } }

        [SerializeField] private GamestateTable<TeamEntry> _teams;
        public GamestateTable<TeamEntry> teams { get { return _teams; } }

        private PhotonView view;




        //  START DEPRECATED
        //  FUCK FUCK FUCK FUCK
        [PunRPC]
        public void UpdateMapDetails(string nextMapName, string nextMapDisplayName)
        {
            nextMap = nextMapName;
            nextMapDisplay = nextMapDisplayName;
        }
        //  END DEPRECATED
    
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
        
            if (PhotonNetwork.IsMasterClient)
            {
                // remove the player from the gamestate tracker
                PlayerEntry playerEntry = players.Get((short)otherPlayer.ActorNumber);
                short team = playerEntry.teamId;


                // remove player from their team
                TeamEntry teamEntry = teams.Get(team);
                if (playerEntry.role == (short) PlayerEntry.Role.Driver)
                {
                    teamEntry.driverId = 0;
                }
                else if (playerEntry.role == (short) PlayerEntry.Role.Gunner)
                {
                    teamEntry.gunnerId = 0;
                }
                teamEntry.Commit();
                playerEntry.Delete();
               
            }
        }


        void Awake()
        {
            GamestatePacketManager.RegisterPacket();

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

                if (succeeded)
                {
                    if(bufferPackets)view.RPC(nameof(ApplyPacket), RpcTarget.OthersBuffered, packet);
                    else view.RPC(nameof(ApplyPacket), RpcTarget.Others, packet);
                    
                }
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

        void Start()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (bufferRpcScenes.Contains(currentSceneName))
            {
                bufferPackets = true;
            }
            else
            {
                bufferPackets = false;
            }
        }
    }
}
