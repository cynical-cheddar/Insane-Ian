using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyButtonScript : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    [SerializeField] private int teamId = 0;
    private Player driverPlayer;
    [SerializeField] private string driverPlayerNickName;
    private Player gunnerPlayer;
    [SerializeField] private string gunnerPlayerNickName;

    public bool driverSlotEmpty = true;
    public bool gunnerSlotEmpty = true;
    
    public void SetParent()
    {
        transform.parent = FindObjectOfType<LobbySlotMaster>().gameObject.transform;
    }

    [PunRPC]
    public void selectDriver(Player selectPlayer)
    {
        if (driverSlotEmpty)
        {
            // clear the player's last slot
            //select the slot
            driverPlayer = selectPlayer;
            driverPlayerNickName = selectPlayer.NickName;
            driverSlotEmpty = false;
        }
    }
    public void selectGunner(Player selectPlayer)
    {
        if (gunnerSlotEmpty)
        {
            // clear the player's last slot
            // get all other LobbyButtons and clear 
            //select the slot
            gunnerPlayer = selectPlayer;
            gunnerPlayerNickName = selectPlayer.NickName;
            gunnerSlotEmpty = false;
        }
    }

    public void setButtonInfo(List<(Player, string, string, string, int)> playersInTeam)
    {
        
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
