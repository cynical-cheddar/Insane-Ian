using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamestate;

public class ScoringHelper : MonoBehaviour {

    int killValue = 10;
    int deathValue = -5;
    int assistValue = 5;

    public int CalcScore(TeamEntry team) {
        return team.kills * killValue + team.deaths * deathValue + team.assists * assistValue;
    }

    public List<TeamEntry> SortTeams() {
        GamestateTracker gamestateTracker = FindObjectOfType<GamestateTracker>();
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