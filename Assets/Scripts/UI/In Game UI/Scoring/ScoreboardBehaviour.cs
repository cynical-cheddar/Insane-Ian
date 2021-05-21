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

    // Called once the network manager deems it ready to display the scores.
    public void StartScoreboard() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        SetUpScoreboard();
    }

    void SetUpScoreboard() {
        for (int i = 0; i < gamestateTracker.teams.count; i++) {
            TeamEntry team = gamestateTracker.teams.GetAtIndex(i);
            team.AddListener(TeamListener);
            team.Release();
            teamPanels[i].gameObject.SetActive(true);
            teamPanels[i].Setup();
        }
        UpdateScores();
    }

    void TeamListener(TeamEntry team) {
        team.Release();
        UpdateScores();
    }

    public void UpdateScores() {
        // Sort teams by score
        List<TeamEntry> sortedTeams = scoringHelper.SortTeams(gamestateTracker);

        // Display teams in order
        for (int i = 0; i < sortedTeams.Count; i++) {
            if (sortedTeams[i].name == null) {
                PlayerEntry driver = gamestateTracker.players.Get(sortedTeams[i].driverId);
                PlayerEntry gunner = gamestateTracker.players.Get(sortedTeams[i].gunnerId);
                teamPanels[i].TeamName.text = $"{driver.name} + {gunner.name}";
                driver.Release();
                gunner.Release();
            } else teamPanels[i].TeamName.text = sortedTeams[i].name;
            teamPanels[i].TeamScore.text = $"Score: {scoringHelper.CalcScore(sortedTeams[i])}";
            teamPanels[i].TeamKDA.text = $"Kills/Deaths: {sortedTeams[i].kills}/{sortedTeams[i].deaths}";
            teamPanels[i].TeamCheckpoints.text = $"Gubbins: {sortedTeams[i].checkpoint}";
            PlayerEntry player = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
            int teamId = player.teamId;
            player.Release();
            if (teamId == sortedTeams[i].id) {
                teamPanels[i].UpdateTransform(true);
            } else {
                teamPanels[i].UpdateTransform(false);
            }
        }
    }
}
