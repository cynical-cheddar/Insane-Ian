using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamestate;

public class ScoringHelper {

    int killValue = 10;
    int deathValue = 0;
    int assistValue = 5;
    int checkpointValue = 1;

    public int CalcScore(TeamEntry team) {
        return team.kills * killValue + team.deaths * deathValue + team.assists * assistValue + team.checkpoint * checkpointValue;
    }

    // Return the teams from the gamestate tracker, sorted by score.
    public List<TeamEntry> SortTeams(GamestateTracker gamestateTracker) {
        List<TeamEntry> unsortedTeams = new List<TeamEntry>();
        for (int i = 0; i < gamestateTracker.teams.count; i++) {
            TeamEntry team = gamestateTracker.teams.GetAtIndex(i);
            unsortedTeams.Add(team);
            team.Release();
        }
        List<TeamEntry> sortedTeams = unsortedTeams;
        sortedTeams.Sort((t1, t2) => CalcScore(t1).CompareTo(CalcScore(t2)));
        sortedTeams.Reverse();

        return sortedTeams;
    }
}