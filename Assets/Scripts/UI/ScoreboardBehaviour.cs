using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardBehaviour : MonoBehaviour
{
    public Text scoreboardText;
    GamestateTracker gamestateTracker;
    List<List<GamestateTracker.PlayerDetails>> playerPairs;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        // Wait for the host to finish loading first
        Invoke("updateScores", 0.1f);
    }

    public void updateScores() {
        string scoreboardDetails = "";
        playerPairs = gamestateTracker.GetPlayerPairs();
        foreach (List<GamestateTracker.PlayerDetails> playerPair in playerPairs) {
            scoreboardDetails += $"Team {playerPair[0].teamId} -- Score: {playerPair[0].score + playerPair[1].score} \n";
            foreach (GamestateTracker.PlayerDetails player in playerPair) {
                scoreboardDetails += $"{player.nickName} -- Score: {player.score} -- K/D/A: {player.kills}/{player.deaths}/{player.assists} \n";
            }
        }
        scoreboardText.text = scoreboardDetails;
    }

}
