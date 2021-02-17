using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

public class NetworkPlayerVehicle : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update

    public PhotonView gunnerPhotonView;
    public PhotonView driverPhotonView;
    public MonoBehaviour[] playerDriverScripts;

    public MonoBehaviour[] playerGunnerScripts;
    
    public MonoBehaviour[] aiDriverScripts;
    public MonoBehaviour[] aiGunnerScripts;

    public bool botDriver = false;
    public bool botGunner = false;

    private string driverNickName = "null";
    private int driverId = 0;
    private string gunnerNickName = "null";
    private int gunnerId = 0;

    public int teamId;

    private GamestateTracker gamestateTracker;


    public string GetGunnerNickName()
    {
        return gunnerNickName;
    }
    public int GetGunnerID()
    {
        return gunnerId;
    }
    public string GetDriverNickName()
    {
        return driverNickName;
    }
    public int GetDriverID()
    {
        return driverId;
    }
    void Start()
    {
        if (FindObjectOfType<GamestateTracker>() != null)
        {
            gamestateTracker = FindObjectOfType<GamestateTracker>();
            gamestateTracker.ForceSynchronisePlayerSchema();
        }
        
    }

    [PunRPC]
    public void SetNetworkTeam_RPC(int newTeamId)
    {
        teamId = newTeamId;
    }

    void EnableMonobehaviours(MonoBehaviour[] scripts)
    {
        
        if (scripts.Length > 0)
        {
            foreach (MonoBehaviour behaviour in scripts)
            {
                //Debug.Log("Enabled monobehaviour " + behaviour.name);
                behaviour.enabled = true;
            }
        }
        
    }


    void TransferGunnerPhotonViewOwnership(GamestateTracker.PlayerDetails gunnerDetails)
    {
        // lookup the player from the gamestate tracker
        if (PhotonNetwork.IsMasterClient)
        {
            gamestateTracker = FindObjectOfType<GamestateTracker>();
            // gamestateTracker.ForceSynchronisePlayerList();
            Player p = gamestateTracker.GetPlayerFromDetails(gunnerDetails);
            //Debug.Log("gunner nickname in transfer: " + p.NickName);
            gunnerPhotonView.TransferOwnership(p);

        }

    }

    void TransferDriverPhotonViewOwnership(GamestateTracker.PlayerDetails driverDetails)
    {
        // lookup the player from the gamestate tracker
        if (PhotonNetwork.IsMasterClient)
        {
            gamestateTracker = FindObjectOfType<GamestateTracker>();
            // gamestateTracker.ForceSynchronisePlayerList();
            Player p = gamestateTracker.GetPlayerFromDetails(driverDetails);
            //Debug.Log("Player p in driver transfer: " + p.ToString() + " name: " + p.NickName);
            //Debug.Log("driver in transfer: " + p.NickName);
            driverPhotonView.TransferOwnership(p);
        }
    }

    // RPC is called on all instances of the game  by Network Manager
    // Handles script separation and the likes
    [PunRPC]
    public void AssignPairDetailsToVehicle(string serializedPlayer1, string serializedPlayer2)
    {
        //Debug.Log("GOT HERE -2");
        GamestateTracker.PlayerDetails player1 =
            JsonUtility.FromJson <GamestateTracker.PlayerDetails>(serializedPlayer1);
        GamestateTracker.PlayerDetails player2 =
            JsonUtility.FromJson <GamestateTracker.PlayerDetails>(serializedPlayer2);
        //Debug.Log("GOT HERE -1");
        GamestateTracker.PlayerDetails driverDetails = new GamestateTracker.PlayerDetails();
        GamestateTracker.PlayerDetails gunnerDetails = new GamestateTracker.PlayerDetails();
        //Debug.Log(serializedPlayer1);
        //Debug.Log(serializedPlayer2);
        if (player1.role == "Driver")
        {
            driverDetails = player1;
            gunnerDetails = player2;
        }
        else
        {
            driverDetails = player2;
            gunnerDetails = player1;
        }

        driverNickName = driverDetails.nickName;
        driverId = driverDetails.playerId;
        gunnerNickName = gunnerDetails.nickName;
        gunnerId = gunnerDetails.playerId;
        
        // firstly, if the gunner is a human, transfer the photonview ownership to the player's client
        
        if(!driverDetails.isBot) TransferDriverPhotonViewOwnership(driverDetails);
        if(!gunnerDetails.isBot) TransferGunnerPhotonViewOwnership(gunnerDetails);
        
        // transfer control to master client if bot
        if (driverDetails.isBot) driverPhotonView.TransferOwnership(PhotonNetwork.MasterClient);
        if (gunnerDetails.isBot) gunnerPhotonView.TransferOwnership(PhotonNetwork.MasterClient);

            // check if the driver is a human or a bot
        if (driverDetails.isBot) botDriver = true;
        //Debug.Log("GOT HERE 0");
        // if they are a bot, then get the MASTER CLIENT to turn on ai controls
        if (botDriver && PhotonNetwork.IsMasterClient)EnableMonobehaviours(aiDriverScripts);
        // otherwise, find the driver player by their nickname. Tell their client to turn on player driver controls
        //Debug.Log("My local name is " + PhotonNetwork.LocalPlayer.NickName);
        if(PhotonNetwork.LocalPlayer.ActorNumber == driverDetails.playerId) EnableMonobehaviours(playerDriverScripts);
        //Debug.Log("GOT HERE");
        // Do the same again for the gunner
        if (gunnerDetails.isBot) botGunner = true;
        if (botGunner && PhotonNetwork.IsMasterClient)EnableMonobehaviours(aiGunnerScripts);
        if(PhotonNetwork.LocalPlayer.ActorNumber == gunnerDetails.playerId) EnableMonobehaviours(playerGunnerScripts);
        //Debug.Log("GOT HERE2");
    }
}
