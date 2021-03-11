using System.Collections;
using System.Collections.Generic;
using Gamestate;
using UnityEngine;

public class GamestateTrackerUtils : MonoBehaviour
{
    private GamestateTracker gamestateTracker;


    void Start()
    {
        gamestateTracker = GetComponent<GamestateTracker>();
    }
    public bool AllPlayerTeamsHaveVehicles()
    {
        bool success = true;
        
        // loop through all teams
        
        // if a team has a human player on it, check if they have selected a vehicle
        
        // if they have not, then fail

        for (int i = 0; i < gamestateTracker.teams.count; i++)
        {
            bool hasHuman = false;
            TeamEntry teamEntry = gamestateTracker.teams.ReadAtIndex((short) i);
            PlayerEntry driverEntry = gamestateTracker.players.Read(teamEntry.driverId);
            PlayerEntry gunnerEntry = gamestateTracker.players.Read(teamEntry.gunnerId);

            if (!driverEntry.isBot) hasHuman = true;
            if (!gunnerEntry.isBot) hasHuman = true;

            if (hasHuman)
            {
                if (teamEntry.hasSelectedVehicle == false) {success = false; break;}
            }
        }

        return success;
    }

    public List<short> GetHumanTeamsIds()
    {
        List<short> idList = new List<short>();
        for (int i = 0; i < gamestateTracker.teams.count; i++)
        {
            bool hasHuman = false;
            TeamEntry teamEntry = gamestateTracker.teams.ReadAtIndex((short) i);
            PlayerEntry driverEntry = gamestateTracker.players.Read(teamEntry.driverId);
            PlayerEntry gunnerEntry = gamestateTracker.players.Read(teamEntry.gunnerId);

            if (!driverEntry.isBot) hasHuman = true;
            if (!gunnerEntry.isBot) hasHuman = true;

            if (hasHuman)
            {
                idList.Add(teamEntry.id);
            }
        }

        return idList;
    }
    public List<short> GetSelectedHumanTeamsIds()
    {
        List<short> idList = new List<short>();
        for (int i = 0; i < gamestateTracker.teams.count; i++)
        {
            bool hasHuman = false;
            TeamEntry teamEntry = gamestateTracker.teams.ReadAtIndex((short) i);
            PlayerEntry driverEntry = gamestateTracker.players.Read(teamEntry.driverId);
            PlayerEntry gunnerEntry = gamestateTracker.players.Read(teamEntry.gunnerId);

            if (!driverEntry.isBot) hasHuman = true;
            if (!gunnerEntry.isBot) hasHuman = true;

            if (hasHuman && teamEntry.hasSelectedVehicle)
            {
                idList.Add(teamEntry.id);
            }
        }

        return idList;
    }
}
