using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Gamestate;
using System.Runtime.InteropServices;
using TMPro;

public class PlinthManager : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void unmute();

    public List<TextMesh> plinthTexts;
    public List<Transform> spawnpoints;
    public TextMeshProUGUI scoreboardText;
    public string defaultVehiclePrefabName;
    GamestateTracker gamestateTracker;
    readonly ScoringHelper scoringHelper = new ScoringHelper();
    List<TeamEntry> sortedTeams;
    public string returnToMenuScene = "MainMenu";

    // Start is called before the first frame update
    void Start() {
        #if UNITY_WEBGL && !UNITY_EDITOR
        unmute();
        #endif
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
                vehiclePrefabName = "VehicleDummyPrefabs/" + vehicleNames[sortedTeams[i].vehicle];
            }

            object[] instantiationData = new object[] { (int)sortedTeams[i].id };

            PhotonNetwork.Instantiate(vehiclePrefabName, spawnpoints[i].position, spawnpoints[i].rotation, 0, instantiationData);
        }
    }

    string GetTeamName(TeamEntry team) {
        string name;
        if (team.name == null) {
            PlayerEntry driver = gamestateTracker.players.Get(team.driverId);
            PlayerEntry gunner = gamestateTracker.players.Get(team.gunnerId);
            name = $"{driver.name} + {gunner.name}";
            driver.Release();
            gunner.Release();
        } else name = team.name;
        return name;
    }

    void UpdateText() {
        // Sort teams by score
        sortedTeams = scoringHelper.SortTeams(gamestateTracker);

        if (PhotonNetwork.IsMasterClient) SpawnPlayerVehicles();

        plinthTexts[0].text = GetTeamName(sortedTeams[0]);
        if (sortedTeams.Count > 1) plinthTexts[1].text = GetTeamName(sortedTeams[1]);
        if (sortedTeams.Count > 2) plinthTexts[2].text = GetTeamName(sortedTeams[2]);

        string newText = "";
        foreach (TeamEntry team in sortedTeams) {
            newText += $"{GetTeamName(team)} -- Score: {scoringHelper.CalcScore(team)} -- Kills: {team.kills} -- Deaths: {team.deaths} -- Gubbins: {team.checkpoint}\n";
        }
        scoreboardText.text = newText;
    }

    public void ReturnToMainMenu()
    {
       // PhotonNetwork.Disconnect();
        SceneManager.LoadScene(returnToMenuScene);
    }

}
