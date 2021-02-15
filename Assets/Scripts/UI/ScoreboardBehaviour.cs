using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ScoreboardBehaviour : MonoBehaviour
{
    public List<TeamPanelBehaviour> teamPanels;
    public List<Sprite> positionImages;
    GamestateTracker gamestateTracker;
    public int killValue, deathValue, assistValue;
    bool scoreboardIsExpanded = false;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        
        // Wait for the host to finish loading first
        Invoke("UpdateScores", 0.1f);
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

    int CalcScore(GamestateTracker.TeamDetails team) {
        return team.kills * killValue + team.deaths * deathValue + team.assists * assistValue;
    }

    List<GamestateTracker.TeamDetails> SortTeams(List<GamestateTracker.TeamDetails> unsortedTeams) {
        // Sort the list, not a stable sort
        List<GamestateTracker.TeamDetails> sortedTeams = unsortedTeams;
        sortedTeams.Sort((t1, t2) => CalcScore(t1).CompareTo(CalcScore(t2)));
        sortedTeams.Reverse();

        // Make sure teams with the same score are sorted
        List<List<GamestateTracker.TeamDetails>> tempList = new List<List<GamestateTracker.TeamDetails>>();
        for (int i = 0; i < sortedTeams.Count; i++) tempList.Add(new List<GamestateTracker.TeamDetails>());
        int score = CalcScore(sortedTeams[0]);
        int counter = 0;
        tempList[0].Add(sortedTeams[0]);
        // Split teams into groups of same score
        for (int i = 1; i < sortedTeams.Count; i++) {
            if (score == CalcScore(sortedTeams[i])) {
                tempList[counter].Add(sortedTeams[i]);
            } else {
                counter++;
                score = CalcScore(sortedTeams[i]);
                tempList[counter].Add(sortedTeams[i]);
            }
        }
        // Sort groups by teamId
        for (int i = 0; i < tempList.Count; i++) {
            tempList[i].Sort((t1, t2) => t1.teamId.CompareTo(t2.teamId));
        }

        // Add the now fully sorted teams to a list to return
        List<GamestateTracker.TeamDetails> returnList = new List<GamestateTracker.TeamDetails>();
        foreach (List<GamestateTracker.TeamDetails> teamGroup in tempList) {
            foreach (GamestateTracker.TeamDetails team in teamGroup) {
                returnList.Add(team);
            }
        }

        return returnList;
    }

    public void UpdateScores() {
        // Sort teams by score
        List<GamestateTracker.TeamDetails> sortedTeams = SortTeams(gamestateTracker.schema.teamsList);
        
        if (scoreboardIsExpanded) {
            // Display teams in order
            for (int i = 0; i < sortedTeams.Count; i++) {
                teamPanels[i].TeamName.text = $"Team {sortedTeams[i].teamId}";
                teamPanels[i].TeamScore.text = $"Score: {CalcScore(sortedTeams[i])}";
                teamPanels[i].TeamKDA.text = $"K/D/A: {sortedTeams[i].kills}/{sortedTeams[i].deaths}/{sortedTeams[i].assists}";
            }
            teamPanels[0].Position.sprite = positionImages[0];
            teamPanels[0].PositionShadow.sprite = positionImages[0];
        } else {
            // Display player's team score
            int teamId = gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber).teamId;
            GamestateTracker.TeamDetails team = gamestateTracker.getTeamDetails(teamId);
            teamPanels[0].TeamName.text = $"Team {teamId}";
            teamPanels[0].TeamScore.text = $"Score: {CalcScore(team)}";
            teamPanels[0].TeamKDA.text = $"K/D/A: {team.kills}/{team.deaths}/{team.assists}";
            for (int i = 0; i < sortedTeams.Count; i++) {
                if (sortedTeams[i].teamId == teamId) {
                    teamPanels[0].Position.sprite = positionImages[i];
                    teamPanels[0].PositionShadow.sprite = positionImages[i];
                }
            }
        }
    }

}
