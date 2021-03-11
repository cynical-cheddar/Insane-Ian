using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Gamestate;

public class ScoreboardBehaviour : MonoBehaviour
{
    public List<TeamPanelBehaviour> teamPanels;
    public List<Sprite> positionImages;
    GamestateTracker gamestateTracker;
    bool scoreboardIsExpanded = false;
    readonly ScoringHelper scoringHelper = new ScoringHelper();

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        
        // Wait for the host to finish loading first
        //Invoke(nameof(UpdateScores), 0.1f);
        UpdateScores();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            ToggleExpandedScoreboard();
        }
    }

    void ToggleExpandedScoreboard() {
        scoreboardIsExpanded = !scoreboardIsExpanded;
        for (int i = 1; i < gamestateTracker.schema.teamsList.Count; i++) {
            teamPanels[i].gameObject.SetActive(scoreboardIsExpanded);
        }
        UpdateScores();
    }

    public void UpdateScores() {
        // Sort teams by score
        List<GamestateTracker.TeamDetails> sortedTeams = scoringHelper.SortTeams(gamestateTracker.schema.teamsList);
        
        if (scoreboardIsExpanded) {
            // Display teams in order
            for (int i = 0; i < sortedTeams.Count; i++) {
                teamPanels[i].TeamName.text = $"Team {sortedTeams[i].teamId}";
                teamPanels[i].TeamScore.text = $"Score: {scoringHelper.CalcScore(sortedTeams[i])}";
                teamPanels[i].TeamKDA.text = $"K/D/A: {sortedTeams[i].kills}/{sortedTeams[i].deaths}/{sortedTeams[i].assists}";
                if (gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber).teamId == sortedTeams[i].teamId) {
                    teamPanels[i].Glow.enabled = true;
                } else {
                    teamPanels[i].Glow.enabled = false;
                }
            }
            teamPanels[0].Position.sprite = positionImages[0];
            teamPanels[0].PositionShadow.sprite = positionImages[0];
        } else {
            // Display player's team score
            int teamId = gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber).teamId;
            GamestateTracker.TeamDetails team = gamestateTracker.getTeamDetails(teamId);
            teamPanels[0].TeamName.text = $"Team {teamId}";
            teamPanels[0].TeamScore.text = $"Score: {scoringHelper.CalcScore(team)}";
            teamPanels[0].TeamKDA.text = $"K/D/A: {team.kills}/{team.deaths}/{team.assists}";
            teamPanels[0].Glow.enabled = false;
            for (int i = 0; i < sortedTeams.Count; i++) {
                if (sortedTeams[i].teamId == teamId) {
                    teamPanels[0].Position.sprite = positionImages[i];
                    teamPanels[0].PositionShadow.sprite = positionImages[i];
                }
            }
        }
    }

}
