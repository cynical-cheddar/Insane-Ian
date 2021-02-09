using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class ScoreboardTestingBehaviour : MonoBehaviour
{
    GamestateTracker gamestateTracker;

    public Dropdown playerChoice;
    public Dropdown statChoice;
    public InputField value;

    private void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        // Give the host a chance to load.
        Invoke("populateOptions", 0.1f);
    }

    void populateOptions() {
        List<string> options = new List<string>();
        foreach (GamestateTracker.PlayerDetails playerDetails in gamestateTracker.schema.playerList) {
            options.Add(playerDetails.nickName);
        }
        playerChoice.AddOptions(options);
    }

    public void updateValue() {
        GamestateTracker.PlayerDetails playerDetails = gamestateTracker.getPlayerDetails(playerChoice.captionText.text);

        if (statChoice.captionText.text == "Score") {
            playerDetails.score += int.Parse(value.text);
        } else if (statChoice.captionText.text == "Kills") {
            playerDetails.kills += int.Parse(value.text);
        } else if (statChoice.captionText.text == "Deaths") {
            playerDetails.deaths += int.Parse(value.text);
        } else if (statChoice.captionText.text == "Assists") {
            playerDetails.assists += int.Parse(value.text);
        }

        gamestateTracker.UpdatePlayerWithNewRecord(playerChoice.captionText.text, JsonUtility.ToJson(playerDetails));
    }
}
