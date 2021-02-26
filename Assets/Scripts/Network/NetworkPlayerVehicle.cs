using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

public class NetworkPlayerVehicle : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    // Start is called before the first frame update

    public PhotonView gunnerPhotonView;
    public PhotonView driverPhotonView;
    private MonoBehaviour[] playerDriverScripts;

    private MonoBehaviour[] playerGunnerScripts;

    private MonoBehaviour[] aiDriverScripts;
    private MonoBehaviour[] aiGunnerScripts;

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

    void Start() {
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

            Weapon[] weapons = GetComponentsInChildren<Weapon>();
            foreach (Weapon weapon in weapons) {
                weapon.gameObject.GetComponent<PhotonView>().TransferOwnership(p);
            }
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

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info) {
        GetComponent<VehicleManager>().SetupVehicleManager();

        gamestateTracker = FindObjectOfType<GamestateTracker>();

        teamId = (int)info.photonView.InstantiationData[0];

        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>(true);
        List<MonoBehaviour> playerDriverScriptsList = new List<MonoBehaviour>();
        List<MonoBehaviour> playerGunnerScriptsList = new List<MonoBehaviour>();
        List<MonoBehaviour> aiDriverScriptsList = new List<MonoBehaviour>();
        List<MonoBehaviour> aiGunnerScriptsList = new List<MonoBehaviour>();
        
        foreach (MonoBehaviour script in scripts) {
             object[] vehicleScriptAttributes = script.GetType().GetCustomAttributes(typeof(VehicleScript), false);
             foreach (object attribute in vehicleScriptAttributes) {
                 VehicleScript vehicleScript = attribute as VehicleScript;
                 if (vehicleScript == null) Debug.LogWarning("Non-VehicleScript script picked up");
                 else {
                     if (vehicleScript.scriptType == ScriptType.playerDriverScript) playerDriverScriptsList.Add(script);
                     if (vehicleScript.scriptType == ScriptType.playerGunnerScript) playerGunnerScriptsList.Add(script);
                     if (vehicleScript.scriptType == ScriptType.aiDriverScript) aiDriverScriptsList.Add(script);
                     if (vehicleScript.scriptType == ScriptType.aiGunnerScript) aiGunnerScriptsList.Add(script);
                 }
             }
        }

        playerDriverScripts = playerDriverScriptsList.ToArray();
        playerGunnerScripts = playerGunnerScriptsList.ToArray();
        aiDriverScripts = aiDriverScriptsList.ToArray();
        aiGunnerScripts = aiGunnerScriptsList.ToArray();

        GamestateTracker.PlayerDetails driverDetails = gamestateTracker.GetPlayerWithDetails(role: "Driver", teamId: teamId);
        GamestateTracker.PlayerDetails gunnerDetails = gamestateTracker.GetPlayerWithDetails(role: "Gunner", teamId: teamId);

        driverNickName = driverDetails.nickName;
        driverId = driverDetails.playerId;
        gunnerNickName = gunnerDetails.nickName;
        gunnerId = gunnerDetails.playerId;
        
        // firstly, if the gunner is a human, transfer the photonview ownership to the player's client
        
        if (!driverDetails.isBot) TransferDriverPhotonViewOwnership(driverDetails);
        if (!gunnerDetails.isBot) TransferGunnerPhotonViewOwnership(gunnerDetails);
        
        // transfer control to master client if bot
        if (driverDetails.isBot) driverPhotonView.TransferOwnership(PhotonNetwork.MasterClient);
        if (gunnerDetails.isBot) gunnerPhotonView.TransferOwnership(PhotonNetwork.MasterClient);

            // check if the driver is a human or a bot
        if (driverDetails.isBot) botDriver = true;
        //Debug.Log("GOT HERE 0");
        // if they are a bot, then get the MASTER CLIENT to turn on ai controls
        if (botDriver && PhotonNetwork.IsMasterClient) EnableMonobehaviours(aiDriverScripts);
        // otherwise, find the driver player by their nickname. Tell their client to turn on player driver controls
        //Debug.Log("My local name is " + PhotonNetwork.LocalPlayer.NickName);
        if(PhotonNetwork.LocalPlayer.ActorNumber == driverDetails.playerId) EnableMonobehaviours(playerDriverScripts);
        //Debug.Log("GOT HERE");
        // Do the same again for the gunner
        if (gunnerDetails.isBot) botGunner = true;
        if (botGunner && PhotonNetwork.IsMasterClient) EnableMonobehaviours(aiGunnerScripts);
        if(PhotonNetwork.LocalPlayer.ActorNumber == gunnerDetails.playerId) EnableMonobehaviours(playerGunnerScripts);
        //Debug.Log("GOT HERE2");

        GetComponentInChildren<GunnerWeaponManager>().SelectFirst();
    }
}
