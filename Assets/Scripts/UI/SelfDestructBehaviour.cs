using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class SelfDestructBehaviour : MonoBehaviour
{
    GamestateTracker gamestateTracker;
    public Text title;
    public Text timeLeftText;
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
                int teamId = gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber).teamId;
                foreach (VehicleManager vehicle in FindObjectsOfType<VehicleManager>()) {
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
