using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardBehaviour : MonoBehaviour
{
    public List<TeamPanelBehaviour> teamPanels;
    GamestateTracker gamestateTracker;
    public int killValue, deathValue, assistValue;
    bool scoreboardIsExpanded = false;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        
        // Wait for the host to finish loading first
        Invoke("updateScores", 0.1f);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            toggleExpandedScoreboard();
        }
    }

    void toggleExpandedScoreboard() {
        scoreboardIsExpanded = !scoreboardIsExpanded;
        for (int i = 1; i < teamPanels.Count; i++) {
            teamPanels[i].gameObject.SetActive(scoreboardIsExpanded);
        }
        updateScores();
    }

    public void updateScores() {
        if (scoreboardIsExpanded) {
            for (int i = 0; i < teamPanels.Count; i++) {
                GamestateTracker.TeamDetails team = gamestateTracker.getTeamDetails(i+1);
                int score = team.kills * killValue + team.deaths * deathValue + team.assists * assistValue;
                teamPanels[i].TeamName.text = $"Team {i+1}";
                teamPanels[i].TeamScore.text = $"Score: {score}";
                teamPanels[i].TeamKDA.text = $"K/D/A: {team.kills}/{team.deaths}/{team.assists}";
            }
        } else {
            int teamId = gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber).teamId;
            GamestateTracker.TeamDetails team = gamestateTracker.getTeamDetails(teamId);
            int score = team.kills * killValue + team.deaths * deathValue + team.assists * assistValue;
            teamPanels[0].TeamName.text = $"Team {teamId}";
            teamPanels[0].TeamScore.text = $"Score: {score}";
            teamPanels[0].TeamKDA.text = $"K/D/A: {team.kills}/{team.deaths}/{team.assists}";
        }
    }

}
