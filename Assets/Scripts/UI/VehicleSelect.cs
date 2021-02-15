using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;

public class VehicleSelect : MonoBehaviour
{
    private Dropdown dropdown;
    private GameObject[] vehicles;
    private GameObject selectedVehicle;
    private GamestateTracker gamestateTracker;
    private int currentTeamId = 0;

    // Start is called before the first frame update
    void Start()
    {
        gamestateTracker = FindObjectOfType<GamestateTracker>();

        dropdown = GetComponent<Dropdown>();
        dropdown.ClearOptions();

        vehicles = Resources.LoadAll<GameObject>("VehiclePrefabs");

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < vehicles.Length; i++) {
            options.Add(new Dropdown.OptionData(vehicles[i].name));
        }
        dropdown.AddOptions(options);

        selectedVehicle = vehicles[0];
    }

    public void SelectVehicle(int i) {
        selectedVehicle = vehicles[i];

        int teamId = gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber).teamId;
        if (teamId != 0) {
            GamestateTracker.TeamDetails teamDetails = gamestateTracker.getTeamDetails(teamId);
            teamDetails.vehiclePrefabName = "VehiclePrefabs/" + selectedVehicle.name;
            if (PhotonNetwork.IsMasterClient) gamestateTracker.UpdateTeamWithNewRecord(teamId, teamDetails);
            else gamestateTracker.gameObject.GetComponent<PhotonView>().RPC("UpdateTeamWithNewRecord", RpcTarget.All, teamId, JsonUtility.ToJson(teamDetails));
        }
    }

    // Update is called once per frame
    void Update()
    {
        // I cannot be arsed to make this efficient.

        GamestateTracker.PlayerDetails me = gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber);

        bool changedTeam = (me.teamId != currentTeamId);
        currentTeamId = me.teamId;

        bool interactable = false;
        if (me.role == "Driver") interactable = true;
        else {
            if (me.role == "Gunner") {
                GamestateTracker.PlayerDetails them = gamestateTracker.GetPlayerWithDetails(role: "Driver", teamId: me.teamId);
                interactable = !(them.role == "Driver" && !them.isBot);
            }
        }

        GamestateTracker.TeamDetails teamDetails = gamestateTracker.getTeamDetails(me.teamId);
        if (changedTeam && interactable) {
            Debug.Log("AAA");
            teamDetails.vehiclePrefabName = "VehiclePrefabs/" + selectedVehicle.name;
            if (PhotonNetwork.IsMasterClient) gamestateTracker.UpdateTeamWithNewRecord(me.teamId, teamDetails);
            else gamestateTracker.gameObject.GetComponent<PhotonView>().RPC("UpdateTeamWithNewRecord", RpcTarget.All, me.teamId, JsonUtility.ToJson(teamDetails));
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

        dropdown.interactable = interactable;
    }
}
