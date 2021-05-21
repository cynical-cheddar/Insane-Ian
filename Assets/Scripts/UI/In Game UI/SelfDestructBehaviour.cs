using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Gamestate;
using TMPro;

public class SelfDestructBehaviour : MonoBehaviour
{
    GamestateTracker gamestateTracker;
    public TextMeshProUGUI title;
    public TextMeshProUGUI timeLeftText;
    public float timeToDestruct = 3f;
    float timeLeft;
    bool shouldBlowUp;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        timeLeft = timeToDestruct;
        timeLeftText.text = timeLeft.ToString();
    }

    // Update is called once per frame
    void Update() {
        // Display a countdown on the UI and blow up the car if K is held for three seconds.
        if (Input.GetKeyDown(KeyCode.K)) {
            shouldBlowUp = true;
            title.gameObject.SetActive(true);
            timeLeftText.gameObject.SetActive(true);
        }   
        if (Input.GetKeyUp(KeyCode.K)) {
            shouldBlowUp = false;
            title.gameObject.SetActive(false);
            timeLeftText.gameObject.SetActive(false);
        }

        if (shouldBlowUp) {
            timeLeft -= Time.deltaTime;
            timeLeftText.text = Mathf.RoundToInt(timeLeft).ToString();
            if (timeLeft < 0) {
                PlayerEntry entry = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
                int teamId = entry.teamId;
                entry.Release();

                foreach (VehicleHealthManager vehicle in FindObjectsOfType<VehicleHealthManager>()) {
                    if (vehicle.teamId == teamId) {
                        vehicle.TakeDamage(99999);
                        shouldBlowUp = false;
                        title.gameObject.SetActive(false);
                        timeLeftText.gameObject.SetActive(false);
                    }
                }
            }
        } else {
            timeLeft = timeToDestruct;
        }

    }

}
