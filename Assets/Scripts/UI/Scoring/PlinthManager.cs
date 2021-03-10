using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlinthManager : MonoBehaviour
{
    public List<TextMesh> plinthTexts;
    public List<Transform> spawnpoints;
    public TextMesh scoreboardText;
    public string defaultVehiclePrefabName;
    GamestateTracker gamestateTracker;
    readonly ScoringHelper scoringHelper = new ScoringHelper();
    List<GamestateTracker.TeamDetails> sortedTeams;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        // Wait for the host to finish loading first
        //Invoke(nameof(UpdateText), 0.1f);
        UpdateText();
        Cursor.lockState = CursorLockMode.None;
    }

    void SpawnPlayerVehicles() {
        for (int i = 0; i < Mathf.Min(sortedTeams.Count, spawnpoints.Count); i++) {
            object[] instantiationData = new object[] { sortedTeams[i].teamId };
            if (sortedTeams[i].vehiclePrefabName != "" && sortedTeams[i].vehiclePrefabName != null && sortedTeams[i].vehiclePrefabName != "null") {
                PhotonNetwork.Instantiate(sortedTeams[i].vehiclePrefabName, spawnpoints[i].position, spawnpoints[i].rotation, 0, instantiationData);
            } else {
                PhotonNetwork.Instantiate(defaultVehiclePrefabName, spawnpoints[i].position, spawnpoints[i].rotation, 0, instantiationData);
            }
        }
    }

    void UpdateText() {
        sortedTeams = scoringHelper.SortTeams(gamestateTracker.schema.teamsList);
        if (PhotonNetwork.IsMasterClient) SpawnPlayerVehicles();
        plinthTexts[0].text = $"Team {sortedTeams[0].teamId}";
        if (sortedTeams.Count > 1) plinthTexts[1].text = $"Team {sortedTeams[1].teamId}";
        if (sortedTeams.Count > 2) plinthTexts[2].text = $"Team {sortedTeams[2].teamId}";

        string newText = "";
        foreach (GamestateTracker.TeamDetails team in sortedTeams) {
            newText += $"Team {team.teamId} -- Score: {scoringHelper.CalcScore(team)} -- K/D/A: {team.kills}/{team.deaths}/{team.assists}\n";
        }
        scoreboardText.text = newText;
    }

    public void ReturnToMainMenu() {
        SceneManager.LoadScene(0);
    }
}
