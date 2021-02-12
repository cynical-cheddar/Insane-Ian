using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlinthManager : MonoBehaviour
{
    public List<TextMesh> plinthTexts;
    GamestateTracker gamestateTracker;
    [SerializeField] List<List<GamestateTracker.PlayerDetails>> playerPairs;
    [SerializeField] List<List<GamestateTracker.PlayerDetails>> topThreePlayerPairs;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        topThreePlayerPairs = new List<List<GamestateTracker.PlayerDetails>>();
        // Wait for the host to finish loading first
        //Invoke("updatePlinths", 0.1f);
    }
    /*
    void calculateTopThreeTeams() {
        List<List<GamestateTracker.PlayerDetails>> sortedPlayerPairs = playerPairs;
        sortedPlayerPairs.Sort((x, y) => (x[0].score + x[1].score).CompareTo(y[0].score + y[1].score));
        topThreePlayerPairs.Add(sortedPlayerPairs[0]);
        if (sortedPlayerPairs.Count > 1) topThreePlayerPairs.Add(sortedPlayerPairs[1]);
        if (sortedPlayerPairs.Count > 2) topThreePlayerPairs.Add(sortedPlayerPairs[2]);
    }

    void updatePlinths() {
        playerPairs = gamestateTracker.GetPlayerPairs();
        calculateTopThreeTeams();
        plinthTexts[0].text = $"Team {topThreePlayerPairs[0][0].teamId}";
        if (topThreePlayerPairs.Count > 1) plinthTexts[1].text = $"Team {topThreePlayerPairs[1][0].teamId}";
        if (topThreePlayerPairs.Count > 2) plinthTexts[2].text = $"Team {topThreePlayerPairs[2][0].teamId}";
    }*/
}
