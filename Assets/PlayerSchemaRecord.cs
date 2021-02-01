using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
    public class PlayerSchemaRecord
    {
        
        public (Player player, string nickName, string role, string character, int team) record;
        
        public PlayerSchemaRecord((Player player, string nickName, string role, string character, int team) record)
        {
            
        }
    }