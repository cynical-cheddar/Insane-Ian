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

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        
        // Wait for the host to finish loading first
        Invoke("updateScores", 0.1f);
    }

    public void updateScores() {
        int teamId = gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber).teamId;
        GamestateTracker.TeamDetails team = gamestateTracker.getTeamDetails(teamId);
        int score = team.kills * killValue + team.deaths * deathValue + team.assists * assistValue;
        teamPanels[0].TeamName.text = $"Team {teamId}";
        teamPanels[0].TeamScore.text = $"Score: {score}";
        teamPanels[0].TeamKDA.text = $"K/D/A: {team.kills}/{team.deaths}/{team.assists}";
    }

}
