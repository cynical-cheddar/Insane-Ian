using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
            ToggleExpandedScoreboard();
        }
    }

    void ToggleExpandedScoreboard() {
        scoreboardIsExpanded = !scoreboardIsExpanded;
        for (int i = 1; i < teamPanels.Count; i++) {
            teamPanels[i].gameObject.SetActive(scoreboardIsExpanded);
        }
        UpdateScores();
    }

    public void UpdateScores() {
        if (scoreboardIsExpanded) {
            // Sort teams by score
            List<GamestateTracker.TeamDetails> teams = gamestateTracker.schema.teamsList;
            teams.Sort((t1, t2) => (t1.kills * killValue + t1.deaths * deathValue + t1.assists * assistValue).CompareTo(t2.kills * killValue + t2.deaths * deathValue + t2.assists * assistValue));
            teams.Reverse();

            // Display teams in order
            for (int i = 0; i < teamPanels.Count; i++) {
                int score = teams[i].kills * killValue + teams[i].deaths * deathValue + teams[i].assists * assistValue;
                teamPanels[i].TeamName.text = $"Team {teams[i].teamId}";
                teamPanels[i].TeamScore.text = $"Score: {score}";
                teamPanels[i].TeamKDA.text = $"K/D/A: {teams[i].kills}/{teams[i].deaths}/{teams[i].assists}";
            }
        } else {
            // Display player's team score
            int teamId = gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber).teamId;
            GamestateTracker.TeamDetails team = gamestateTracker.getTeamDetails(teamId);
            int score = team.kills * killValue + team.deaths * deathValue + team.assists * assistValue;
            teamPanels[0].TeamName.text = $"Team {teamId}";
            teamPanels[0].TeamScore.text = $"Score: {score}";
            teamPanels[0].TeamKDA.text = $"K/D/A: {team.kills}/{team.deaths}/{team.assists}";
        }
    }

}
