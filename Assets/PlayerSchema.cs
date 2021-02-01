using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
    public class PlayerSchema
    {
        
        public List<(Player player, string nickName, string role, string character, int team)> schema;
        
        public PlayerSchema(List<(Player player, string nickName, string role, string character, int team)> schema)
        {
            
        }
        
        public void RemovePlayerFromSchema(Player p, PlayerSchema playerSlotSchema)
        {
            foreach ((Player, string, string, string, int) record in playerSlotSchema.schema)
            {
                if (record.Item1.Equals(p))
                {
                    playerSlotSchema.schema.Remove((record));
                }
            }
        }
        
    }