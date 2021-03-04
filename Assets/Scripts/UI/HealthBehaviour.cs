using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using System.Collections.Generic;
using Gamestate;

public class HealthBehaviour : MonoBehaviour {

    public Text healthLabel;
    GamestateTracker gamestateTracker;
    int previousRoundedHealth;
    public GameObject damageIndicator;
    public Transform damageIndicatorInstantiateTransform;
    public int damageTaken;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
    }

    void Update() {
        SetHealth();
    }

    public void SetHealth() {
        List<VehicleManager> vehicles = new List<VehicleManager>(FindObjectsOfType<VehicleManager>());

        PlayerEntry entry = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        int teamId = entry.teamId;
        entry.Release();

        foreach (VehicleManager vehicle in vehicles) {
            if (vehicle.teamId == teamId) {
                if (Mathf.RoundToInt(vehicle.health) != previousRoundedHealth) {
                    damageTaken = Mathf.RoundToInt(vehicle.health) - previousRoundedHealth;
                    Instantiate(damageIndicator, damageIndicatorInstantiateTransform);
                    healthLabel.text = Mathf.RoundToInt(vehicle.health).ToString();
                    previousRoundedHealth = Mathf.RoundToInt(vehicle.health);
                }
                break;
            }
        }
    }
}
