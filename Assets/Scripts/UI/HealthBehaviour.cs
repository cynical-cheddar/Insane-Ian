using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using System.Collections.Generic;
using Gamestate;
using TMPro;

public class HealthBehaviour : MonoBehaviour {

    public TextMeshProUGUI healthLabel;
    GamestateTracker gamestateTracker;
    int previousRoundedHealth = 100;
    public GameObject damageIndicator;
    public Transform damageIndicatorInstantiateTransform;
    public int damageTaken;
    public UiBar healthBar;
    
    

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
    }

    void Update() {
        SetHealth();
    }

    public void SetHealth() {
        List<VehicleHealthManager> vehicles = new List<VehicleHealthManager>(FindObjectsOfType<VehicleHealthManager>());

        PlayerEntry entry = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        int teamId = entry.teamId;
        entry.Release();

        foreach (VehicleHealthManager vehicle in vehicles) {
            if (vehicle.teamId == teamId) {
                if (Mathf.CeilToInt(vehicle.health) != previousRoundedHealth) {
                    damageTaken = Mathf.CeilToInt(vehicle.health) - previousRoundedHealth;
                    Instantiate(damageIndicator, damageIndicatorInstantiateTransform);
                    if (vehicle.health < 0f) vehicle.health = 0f;
                    healthLabel.text = Mathf.CeilToInt(vehicle.health).ToString();
                    previousRoundedHealth = Mathf.CeilToInt(vehicle.health);
                    healthBar.SetProgressBar(vehicle.scaledHealth);
                    healthBar.SetNumber(Mathf.CeilToInt(vehicle.health).ToString());
                    
                }
                break;
            }
        }
    }
}
