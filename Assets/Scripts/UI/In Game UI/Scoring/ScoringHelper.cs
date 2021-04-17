using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamestate;

public class ScoringHelper {

    int killValue = 10;
    int deathValue = -5;
    int assistValue = 5;
    int checkpointValue = 10;

    public int CalcScore(TeamEntry team) {
        Debug.Log(" scoreboard calc score called");
        return team.kills * killValue + team.deaths * deathValue + team.assists * assistValue + team.checkpoint * checkpointValue;
    }

    public List<TeamEntry> SortTeams(GamestateTracker gamestateTracker) {
        Debug.Log(" scoreboard sort teams called");
        List<TeamEntry> unsortedTeams = new List<TeamEntry>();
        for (int i = 0; i < gamestateTracker.teams.count; i++) {
            TeamEntry team = gamestateTracker.teams.GetAtIndex(i);
            unsortedTeams.Add(team);
            team.Release();
            Debug.Log(" scoreboard unsorted team added");
        }
        Debug.Log(" scoreboard all unsorted teams added");
        List<TeamEntry> sortedTeams = unsortedTeams;
        sortedTeams.Sort((t1, t2) => CalcScore(t1).CompareTo(CalcScore(t2)));
        sortedTeams.Reverse();
        Debug.Log(" scoreboard teams sorted");

        return sortedTeams;
    }
}