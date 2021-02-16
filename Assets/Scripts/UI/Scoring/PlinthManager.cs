using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlinthManager : MonoBehaviour
{
    public List<TextMesh> plinthTexts;
    GamestateTracker gamestateTracker;
    ScoringHelper scoringHelper = new ScoringHelper();
    [SerializeField] List<List<GamestateTracker.TeamDetails>> teams;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        // Wait for the host to finish loading first
        Invoke("updatePlinths", 0.1f);
    }

    void updatePlinths() {
        List<GamestateTracker.TeamDetails> sortedTeams = scoringHelper.SortTeams(gamestateTracker.schema.teamsList);
        plinthTexts[0].text = $"Team {sortedTeams[0].teamId}";
        if (teams.Count > 1) plinthTexts[1].text = $"Team {sortedTeams[1].teamId}";
        if (teams.Count > 2) plinthTexts[2].text = $"Team {sortedTeams[2].teamId}";
    }
}
