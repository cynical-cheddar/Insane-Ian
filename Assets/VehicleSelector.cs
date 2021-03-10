using System.Collections;
using System.Collections.Generic;
using Gamestate;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class VehicleSelector : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform spawnPoint;

    private bool priority = false;
    private short otherId = 0;

    private short ourTeamId = 0; 

    private List<string> vehicleNames;

    private string currentVehicle = "";

    private string prefix = "VehiclePrefabs/";

    private GamestateTracker gamestateTracker;
    private GamestateVehicleLookup gamestateVehicleLookup;

    public List<VehicleButtonScript> vehicleButtons;

    private GameObject currentVehicleInstance;
    
    // if you are the driver on the team, or a gunner with only a bot, then you may select the vehicle
    
    // whenever we select a vehicle, destroy the last one (if it exists), then spawn the new one
    // make sure we disable all drive scripts
    
    // send rpcs to all, but reference the gunner id if it is not zero. only actually do the stuff on the gunner end if the id is ours
    
    
    // if we have priority, then allow the buttons to be clickable
    void SetButtonsInteractable(bool set)
    {
        foreach (VehicleButtonScript btn in vehicleButtons)
        {
            btn.GetComponent<Button>().interactable = set;
        }
    }

    public void SelectVehicle(short vehicleId)
    {
        // update the selected team vehicle in the gamestate tracker
        TeamEntry teamEntry = gamestateTracker.teams.Get(ourTeamId);
        teamEntry.vehicle = vehicleId;
        teamEntry.Commit();
        GetComponent<PhotonView>().RPC(nameof(DisplaySelectedVehicle_RPC), RpcTarget.All, vehicleId ,otherId, (short) PhotonNetwork.LocalPlayer.ActorNumber);
        
    }

    [PunRPC]
    void DisplaySelectedVehicle_RPC(short vehicleId, short otherPlayerId, short myId)
    {
        if (otherPlayerId == PhotonNetwork.LocalPlayer.ActorNumber || myId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            currentVehicle = gamestateVehicleLookup.sortedVehicleNames[vehicleId];
            // destroy last vehicle
            if (currentVehicleInstance != null)
            {
                Destroy(currentVehicleInstance);
            }
            // spawn new vehicle
            currentVehicleInstance = Instantiate (Resources.Load (prefix + currentVehicle) as GameObject);
            currentVehicleInstance.transform.position = spawnPoint.position;
            currentVehicleInstance.transform.rotation = spawnPoint.rotation;
        }
    }

    void SetupButtons()
    {
        for (int i = 0; i < vehicleNames.Count; i++)
        {
            vehicleButtons[i].SetupButton(vehicleNames[i], (short) i, this);
        }
    }
    
    
    void Start()
    {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        gamestateVehicleLookup = FindObjectOfType<GamestateVehicleLookup>();
        vehicleNames = gamestateVehicleLookup.sortedVehicleNames;
        
        // get us
        PlayerEntry playerEntry = gamestateTracker.players.Read((short)PhotonNetwork.LocalPlayer.ActorNumber);

        // get our role
        PlayerEntry.Role ourRole = (PlayerEntry.Role)playerEntry.role;


        // get the team we are in
        TeamEntry teamEntry = gamestateTracker.teams.Read((short) playerEntry.teamId);
        ourTeamId = playerEntry.teamId;
       

        // if we are a gunner, check if the driver is a bot.
        // if so, get priority
        if (ourRole == PlayerEntry.Role.Gunner)
        {
            PlayerEntry driverEntry = gamestateTracker.players.Read((short) teamEntry.driverId);
            if (driverEntry.isBot) priority = true;
        }

        // if we are a driver, get priority
        if (ourRole == PlayerEntry.Role.Driver) priority = true;

        
        // assign other player id, only needed if the other player is human
        // hence we only check for if we are a driver
        if (priority && ourRole == PlayerEntry.Role.Driver)
        {
            // get gunner id
            PlayerEntry gunnerEntry = gamestateTracker.players.Read(teamEntry.gunnerId);

            otherId = gunnerEntry.id;
        }

        SetButtonsInteractable(priority);
        SetupButtons();
    }

    
}
