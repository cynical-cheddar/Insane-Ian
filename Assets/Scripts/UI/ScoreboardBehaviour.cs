using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardBehaviour : MonoBehaviour
{
    public Text scoreboardText;
    GamestateTracker gamestateTracker;
    public int killValue, deathValue, assistValue;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        // Wait for the host to finish loading first
        Invoke("updateScores", 0.1f);
    }

    public void updateScores() {
        string scoreboardDetails = "";
        foreach (GamestateTracker.TeamDetails team in gamestateTracker.schema.teamsList) {
            int score = team.kills * killValue + team.deaths * deathValue + team.assists * assistValue;
            scoreboardDetails += $"Team {team.teamID} -- Score: {score} -- K/D/A: {team.kills}/{team.deaths}/{team.assists} \n";
        }
        scoreboardText.text = scoreboardDetails;
    }

}
