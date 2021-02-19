using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using System.Collections.Generic;

public class HealthBehaviour : MonoBehaviour {

    public Text healthLabel;
    GamestateTracker gamestateTracker;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
    }

    void Update() {
        SetHealth();
    }

    public void SetHealth() {
        List<VehicleManager> vehicles = new List<VehicleManager>(FindObjectsOfType<VehicleManager>());
        int teamId = gamestateTracker.getPlayerDetails(PhotonNetwork.LocalPlayer.ActorNumber).teamId;
        foreach (VehicleManager vehicle in vehicles) {
            if (vehicle.teamId == teamId) {
                healthLabel.text = Mathf.RoundToInt(vehicle.health).ToString();
            }
        }
    }
}
