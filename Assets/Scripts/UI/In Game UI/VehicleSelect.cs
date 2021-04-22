using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using Gamestate;

public class VehicleSelect : MonoBehaviour
{
    private Dropdown dropdown;
    private List<string> vehicleNames;
    private string selectedVehicle;
    private GamestateTracker gamestateTracker;
    private GamestateVehicleLookup gamestateVehicleLookup;
    private int currentTeamId = 0;

    // Start is called before the first frame update
    void Start()
    {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        gamestateVehicleLookup = FindObjectOfType<GamestateVehicleLookup>();
        dropdown = GetComponent<Dropdown>();
        dropdown.ClearOptions();

        vehicleNames = gamestateVehicleLookup.sortedVehicleNames;

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < vehicleNames.Count; i++) {
            options.Add(new Dropdown.OptionData(vehicleNames[i]));
        }
        dropdown.AddOptions(options);

        selectedVehicle = vehicleNames[0];
     
        
    }

    public void SelectVehicle(int i) {
        selectedVehicle = vehicleNames[i];

        PlayerEntry playerEntry = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        
        
        int teamId = playerEntry.teamId;
        playerEntry.Release();
        
        if (teamId != 0) {
            // get team 
            TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);
            teamEntry.vehicle = (short) i;
            teamEntry.Commit();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        
        
        
        
        
        
        /*
        
        
        // I cannot be arsed to make this efficient.

        PlayerEntry me = gamestateTracker.players.Read((short)PhotonNetwork.LocalPlayer.ActorNumber);

        bool changedTeam = (me.teamId != currentTeamId);
        currentTeamId = me.teamId;

        bool interactable = false;
        if (me.role == (short) PlayerEntry.Role.Driver) interactable = true;
        else {
            if (me.role == (short) PlayerEntry.Role.Gunner) {
                GamestateTracker.PlayerDetails them = gamestateTracker.GetPlayerWithDetails(role: "Driver", teamId: me.teamId);
                interactable = !(them.role == "Driver" && !them.isBot);
            }
        }

        GamestateTracker.TeamDetails teamDetails = gamestateTracker.getTeamDetails(me.teamId);
        if (changedTeam && interactable) {
            //Debug.Log("AAA");
            teamDetails.vehiclePrefabName = "VehiclePrefabs/" + selectedVehicle.name;
            if (PhotonNetwork.IsMasterClient) gamestateTracker.UpdateTeamWithNewRecord(me.teamId, teamDetails);
            else gamestateTracker.gameObject.GetComponent<PhotonView>().RPC(nameof(UpdateTeamWithNewRecord), RpcTarget.All, me.teamId, JsonUtility.ToJson(teamDetails));
        }

        if (teamDetails.vehiclePrefabName != "VehiclePrefabs/" + selectedVehicle.name) {
            for (int i = 0; i < vehicles.Length; i++) {
                if (teamDetails.vehiclePrefabName == "VehiclePrefabs/" + vehicles[i].name) {
                    selectedVehicle = vehicles[i];
                    dropdown.interactable = true;
                    dropdown.value = i;
                    dropdown.RefreshShownValue();
                    break;
                }
            }
        }

        dropdown.interactable = interactable;*/
    }
}
