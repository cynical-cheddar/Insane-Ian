using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Gamestate;

public class ScoreboardBehaviour : MonoBehaviour {
    public List<TeamPanelBehaviour> teamPanels;
    public List<Sprite> positionImages;
    GamestateTracker gamestateTracker;
    bool scoreboardIsExpanded = false;
    readonly ScoringHelper scoringHelper = new ScoringHelper();

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        UpdateScores();

        for (int i = 0; i < gamestateTracker.teams.count; i++) {
            TeamEntry team = gamestateTracker.teams.GetAtIndex(i);
            team.AddListener(TeamListener);
        }

    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            ToggleExpandedScoreboard();
        }
    }

    void TeamListener(TeamEntry team) {
        UpdateScores();
    }

    void ToggleExpandedScoreboard() {
        scoreboardIsExpanded = !scoreboardIsExpanded;
        for (int i = 1; i < gamestateTracker.teams.count; i++) {
            teamPanels[i].gameObject.SetActive(scoreboardIsExpanded);
        }
        UpdateScores();
    }

    public void UpdateScores() {
        // Sort teams by score
        List<TeamEntry> sortedTeams = scoringHelper.SortTeams(gamestateTracker);

        if (scoreboardIsExpanded) {
            // Display teams in order
            for (int i = 0; i < sortedTeams.Count; i++) {
                teamPanels[i].TeamName.text = sortedTeams[i].name;
                teamPanels[i].TeamScore.text = $"Score: {scoringHelper.CalcScore(sortedTeams[i])}";
                teamPanels[i].TeamKDA.text = $"K/D/A: {sortedTeams[i].kills}/{sortedTeams[i].deaths}/{sortedTeams[i].assists}";
                PlayerEntry player = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
                int teamId = player.teamId;
                player.Release();
                if (teamId == sortedTeams[i].id) {
                    teamPanels[i].Glow.enabled = true;
                } else {
                    teamPanels[i].Glow.enabled = false;
                }
            }
            teamPanels[0].Position.sprite = positionImages[0];
            teamPanels[0].PositionShadow.sprite = positionImages[0];
        } else {
            // Display player's team score
            TeamEntry team = gamestateTracker.teams.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
            teamPanels[0].TeamName.text = team.name;
            teamPanels[0].TeamScore.text = $"Score: {scoringHelper.CalcScore(team)}";
            teamPanels[0].TeamKDA.text = $"K/D/A: {team.kills}/{team.deaths}/{team.assists}";
            teamPanels[0].Glow.enabled = false;
            for (int i = 0; i < sortedTeams.Count; i++) {
                if (sortedTeams[i].id == team.id) {
                    teamPanels[0].Position.sprite = positionImages[i];
                    teamPanels[0].PositionShadow.sprite = positionImages[i];
                }
            }
        }
    }

}