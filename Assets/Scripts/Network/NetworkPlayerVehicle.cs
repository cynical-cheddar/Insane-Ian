using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using Gamestate;

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


    void TransferGunnerPhotonViewOwnership()
    {
        // lookup the player from the gamestate tracker
        if (PhotonNetwork.IsMasterClient)
        {
            gamestateTracker = FindObjectOfType<GamestateTracker>();
            // gamestateTracker.ForceSynchronisePlayerList();
            Player p = PhotonNetwork.CurrentRoom.GetPlayer(gunnerId);
            //Debug.Log("gunner nickname in transfer: " + p.NickName);
            gunnerPhotonView.TransferOwnership(p);

            Weapon[] weapons = GetComponentsInChildren<Weapon>();
            foreach (Weapon weapon in weapons) {
                weapon.gameObject.GetComponent<PhotonView>().TransferOwnership(p);
            }
        }

    }

    void TransferDriverPhotonViewOwnership()
    {
        // lookup the player from the gamestate tracker
        if (PhotonNetwork.IsMasterClient)
        {
            gamestateTracker = FindObjectOfType<GamestateTracker>();
            // gamestateTracker.ForceSynchronisePlayerList();
            Player p = PhotonNetwork.CurrentRoom.GetPlayer(driverId);
            //Debug.Log("Player p in driver transfer: " + p.ToString() + " name: " + p.NickName);
            //Debug.Log("driver in transfer: " + p.NickName);
            driverPhotonView.TransferOwnership(p);
        }
    }

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info) {
        Debug.Log("Spawn start");
        GetComponent<VehicleHealthManager>().SetupVehicleManager();

        gamestateTracker = FindObjectOfType<GamestateTracker>();

        teamId = (int)info.photonView.InstantiationData[0];

        gameObject.name = gameObject.name + teamId;

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

        //GamestateTracker.PlayerDetails driverDetails = gamestateTracker.GetPlayerWithDetails(role: "Driver", teamId: teamId);
        //GamestateTracker.PlayerDetails gunnerDetails = gamestateTracker.GetPlayerWithDetails(role: "Gunner", teamId: teamId);
        PlayerEntry driverEntry = gamestateTracker.players.Find((PlayerEntry entry) => {
            return entry.role   == (short)PlayerEntry.Role.Driver &&
                   entry.teamId == teamId;
        });
        driverNickName = driverEntry.name;
        driverId = driverEntry.id;
        botDriver = driverEntry.isBot;
        
        PlayerEntry gunnerEntry = gamestateTracker.players.Find((PlayerEntry entry) => {
            return entry.role   == (short)PlayerEntry.Role.Gunner &&
                   entry.teamId == teamId;
        });
        gunnerNickName = gunnerEntry.name;
        gunnerId = gunnerEntry.id;
        botGunner = gunnerEntry.isBot;

        
        // firstly, if the gunner is a human, transfer the photonview ownership to the player's client
        
        if (!botDriver) TransferDriverPhotonViewOwnership();
        if (!botGunner) TransferGunnerPhotonViewOwnership();
        
        // transfer control to master client if bot
        if (botDriver) driverPhotonView.TransferOwnership(PhotonNetwork.MasterClient);
        if (botGunner) gunnerPhotonView.TransferOwnership(PhotonNetwork.MasterClient);

        

       

        
        //GetComponentInChildren<GunnerWeaponManager>().SelectFirst();
    }

    [PunRPC]
    public void ActivateVehicleInputs()
    {
        // check if the driver is a human or a bot
  
        // if they are a bot, then get the MASTER CLIENT to turn on ai controls
        if (botDriver && PhotonNetwork.IsMasterClient) EnableMonobehaviours(aiDriverScripts);
        // otherwise, find the driver player by their nickname. Tell their client to turn on player driver controls
        //Debug.Log("My local name is " + PhotonNetwork.LocalPlayer.NickName);
        TeamEntry team = gamestateTracker.teams.Get((short)teamId);
        if (team.name == null) {
            PlayerEntry driver = gamestateTracker.players.Get(team.driverId);
            PlayerEntry gunner = gamestateTracker.players.Get(team.gunnerId);
            GetComponent<TeamNameSetup>().SetupTeamName($"{driver.name} + {gunner.name}");
            driver.Release();
            gunner.Release();
       } else GetComponent<TeamNameSetup>().SetupTeamName("Team " + teamId);


        if (PhotonNetwork.LocalPlayer.ActorNumber == driverId)
        {
            EnableMonobehaviours(playerDriverScripts);
            GetComponent<TeamNameSetup>().SetupTeamName("");
        }
        //Debug.Log("GOT HERE");
        // Do the same again for the gunner
        if (botGunner && PhotonNetwork.IsMasterClient) EnableMonobehaviours(aiGunnerScripts);
        if (PhotonNetwork.LocalPlayer.ActorNumber == gunnerId)
        {
            EnableMonobehaviours(playerGunnerScripts);
            GetComponent<TeamNameSetup>().SetupTeamName("");
        }

        PhysXSceneManager physXSceneManager = FindObjectOfType<PhysXSceneManager>();

        physXSceneManager.doPhysics = true;

        //Debug.Log("GOT HERE2");
        Debug.Log("Spawn success");

        
        GetComponent<DriverAbilityManager>().SetupDriverAbilityManager();
    }
}
