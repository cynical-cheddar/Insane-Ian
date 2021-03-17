using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Gamestate;

public class PlinthManager : MonoBehaviour {
    public List<TextMesh> plinthTexts;
    public List<Transform> spawnpoints;
    public TextMesh scoreboardText;
    public string defaultVehiclePrefabName;
    GamestateTracker gamestateTracker;
    readonly ScoringHelper scoringHelper = new ScoringHelper();
    List<TeamEntry> sortedTeams;
    public string returnToMenuScene = "menu";

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        // Wait for the host to finish loading first
        //Invoke(nameof(UpdateText), 0.1f);
        UpdateText();
        Cursor.lockState = CursorLockMode.None;
    }

    void SpawnPlayerVehicles() {
        List<string> vehicleNames = gamestateTracker.GetComponent<GamestateVehicleLookup>().sortedVehicleNames;

        for (int i = 0; i < Mathf.Min(sortedTeams.Count, spawnpoints.Count); i++) {
            string vehiclePrefabName = defaultVehiclePrefabName;

            if (sortedTeams[i].hasSelectedVehicle) {
                vehiclePrefabName = "VehiclePrefabs/" + vehicleNames[sortedTeams[i].vehicle];
            }

            object[] instantiationData = new object[] { (int)sortedTeams[i].id };

            PhotonNetwork.Instantiate(vehiclePrefabName, spawnpoints[i].position, spawnpoints[i].rotation, 0, instantiationData);
        }
    }

    void UpdateText() {
        // Sort teams by score
        sortedTeams = scoringHelper.SortTeams();

        if (PhotonNetwork.IsMasterClient) SpawnPlayerVehicles();

        plinthTexts[0].text = sortedTeams[0].name;
        if (sortedTeams.Count > 1) plinthTexts[1].text = sortedTeams[1].name;
        if (sortedTeams.Count > 2) plinthTexts[2].text = sortedTeams[2].name;

        string newText = "";
        foreach (TeamEntry team in sortedTeams) {
            newText += $"{team.name} -- Score: {scoringHelper.CalcScore(team)} -- K/D/A: {team.kills}/{team.deaths}/{team.assists}\n";
        }
        scoreboardText.text = newText;
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(returnToMenuScene);
    }

}
