using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

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

    public Text driverPlayerText;
    public Text gunnerPlayerText;
    
    
    
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

    public void setButtonInfo(PlayerSchema playersInTeam)
    {
        foreach (PlayerSchema.Record record in playersInTeam.schema)
        {
            // check if we have a driver
            if (record.role == "Driver")
            {
                driverPlayerText.text = record.nickName;
            }
            // check if we have a driver
            if (record.role == "Gunner")
            {
                driverPlayerText.text = record.nickName;
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
