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
    readonly ScoringHelper scoringHelper = new ScoringHelper();

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        SetUpScoreboard();
    }

    void SetUpScoreboard() {
        for (int i = 0; i < gamestateTracker.teams.count; i++) {
            TeamEntry team = gamestateTracker.teams.GetAtIndex(i);
            team.AddListener(TeamListener);
            team.Release();
            teamPanels[i].gameObject.SetActive(true);
        }
        UpdateScores();
    }

    void TeamListener(TeamEntry team) {
        team.Release();
        UpdateScores();
    }

    public void UpdateScores() {
        // Sort teams by score
        List<TeamEntry> sortedTeams = scoringHelper.SortTeams();

        // Display teams in order
        for (int i = 0; i < sortedTeams.Count; i++) {
            if (sortedTeams[i].name == null) teamPanels[i].TeamName.text = $"Team {sortedTeams[i].id}";
            else teamPanels[i].TeamName.text = sortedTeams[i].name;
            teamPanels[i].TeamScore.text = $"Score: {scoringHelper.CalcScore(sortedTeams[i])}";
            teamPanels[i].TeamKDA.text = $"K/D/A: {sortedTeams[i].kills}/{sortedTeams[i].deaths}/{sortedTeams[i].assists}";
            teamPanels[i].TeamCheckpoints.text = $"Checkpoints: {sortedTeams[i].checkpoint}";
        }
    }

}