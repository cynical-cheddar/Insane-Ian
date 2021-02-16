using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringHelper {

    int killValue = 10;
    int deathValue = -5;
    int assistValue = 5;

    public int CalcScore(GamestateTracker.TeamDetails team) {
        return team.kills * killValue + team.deaths * deathValue + team.assists * assistValue;
    }

    public List<GamestateTracker.TeamDetails> SortTeams(List<GamestateTracker.TeamDetails> unsortedTeams) {
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
}


