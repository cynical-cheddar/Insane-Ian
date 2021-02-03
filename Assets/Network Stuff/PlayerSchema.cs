using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
    public class PlayerSchema
    {
        public List<Record> schema;
        public struct Record
        {
            public Player player;
            public string nickName;
            public string role;
            public string character;
            public int team;
        }
        
 
        
        public PlayerSchema(List<Record> schema)
        {
            this.schema = new List<Record>();
        }
        public PlayerSchema()
        {
            this.schema = new List<Record>();
        }
        
        public PlayerSchema RemovePlayerFromSchema(Player p, PlayerSchema playerSlotSchema)
        {
            foreach (Record record in playerSlotSchema.schema)
            {
                if (record.player.Equals(p))
                {
                    playerSlotSchema.schema.Remove((record));
                }
            }

            return playerSlotSchema;

        }
        
        public PlayerSchema AddPlayerToSchema(Player p, PlayerSchema playerSlotSchema)
        {
            Record newRecord;
            newRecord.player = p;
            newRecord.nickName = p.NickName;
            newRecord.character = "null";
            newRecord.role = "null";
            newRecord.team = 0;
            playerSlotSchema.schema.Add(newRecord);
            return playerSlotSchema;
        }
        
        
        public PlayerSchema UpdatePlayerInSchema(Player p, string role, string character, int team, PlayerSchema playerSlotSchema)
        {
            foreach (Record record in playerSlotSchema.schema)
            {
                
                bool found = false;
                if (record.player.Equals(p))
                {
                    found = true;
                    Record newRecord;
                    newRecord.player = p;
                    newRecord.nickName = p.NickName;
                    newRecord.role = role;
                    newRecord.character = character;
                    newRecord.team = team;
                    
                    playerSlotSchema.schema.Add(newRecord);
                }
                if (found)
                {
                    playerSlotSchema.schema.Remove(record);
                }
            
            }
            return playerSlotSchema;
        
        }
        
        public PlayerSchema GetPlayersFromHostSchemaByTeam(PlayerSchema hostSchema, int teamId)
        {
            PlayerSchema playerList = new PlayerSchema();
            foreach (Record record in hostSchema.schema)
            {
                if (record.team == teamId)
                {
                    playerList.AddPlayerToSchema(record.player, hostSchema);
                }
            }

            return playerList;
        }
        
    }