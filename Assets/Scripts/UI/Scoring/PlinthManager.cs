using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlinthManager : MonoBehaviour
{
    public List<TextMesh> plinthTexts;
    public TextMesh scoreboardText;
    GamestateTracker gamestateTracker;
    ScoringHelper scoringHelper = new ScoringHelper();

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        // Wait for the host to finish loading first
        Invoke(nameof(UpdateText), 0.1f);
    }

    void UpdateText() {

        List<GamestateTracker.TeamDetails> sortedTeams = scoringHelper.SortTeams(gamestateTracker.schema.teamsList);
        plinthTexts[0].text = $"Team {sortedTeams[0].teamId}";
        if (sortedTeams.Count > 1) plinthTexts[1].text = $"Team {sortedTeams[1].teamId}";
        if (sortedTeams.Count > 2) plinthTexts[2].text = $"Team {sortedTeams[2].teamId}";

        string newText = "";
        foreach (GamestateTracker.TeamDetails team in sortedTeams) {
            newText += $"Team {team.teamId} -- Score: {scoringHelper.CalcScore(team)} -- K/D/A: {team.kills}/{team.deaths}/{team.assists}\n";
        }
        scoreboardText.text = newText;
    }
}
