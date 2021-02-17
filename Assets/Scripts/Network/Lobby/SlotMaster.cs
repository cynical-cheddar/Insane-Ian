using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMaster : MonoBehaviour
{

    GamestateTracker gamestateTracker;
    public LobbyButtonScript lbs;

    // Start is called before the first frame update
    void Start()
    {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        gamestateTracker.schema.teamsList.Add(new GamestateTracker.TeamDetails(lbs.teamId));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
