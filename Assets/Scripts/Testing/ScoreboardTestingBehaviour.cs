using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class ScoreboardTestingBehaviour : MonoBehaviour
{
    GamestateTracker gamestateTracker;

    public Dropdown playerChoice;
    public Dropdown statChoice;
    public InputField value;

    private void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        // Give the host a chance to load.
        Invoke("populateOptions", 0.1f);
    }

    void populateOptions() {
        List<string> options = new List<string>();
        foreach (GamestateTracker.TeamDetails team in gamestateTracker.schema.teamsList) {
            options.Add(team.teamID.ToString());
        }
        playerChoice.AddOptions(options);
    }

    public void updateValue() {
        GamestateTracker.TeamDetails team = gamestateTracker.getTeamDetails(int.Parse(playerChoice.captionText.text));
        
        if (statChoice.captionText.text == "Kills") {
            team.kills += int.Parse(value.text);
        } else if (statChoice.captionText.text == "Deaths") {
            team.deaths += int.Parse(value.text);
        } else if (statChoice.captionText.text == "Assists") {
            team.assists += int.Parse(value.text);
        }

        gamestateTracker.UpdateTeamWithNewRecord(team.teamID, JsonUtility.ToJson(team));
    }

    public void EndGame() {
        PhotonNetwork.LoadLevel("GameOver");
    }

    public void damageVehicle() {
        int teamId = 1;
        float amount = 25f;
        VehicleStats[] vehicles = FindObjectsOfType<VehicleStats>();
        foreach (VehicleStats vehicle in vehicles) {
            if (teamId == vehicle.teamId) {
                vehicle.takeDamage(amount);
            }
        }
    }

}
