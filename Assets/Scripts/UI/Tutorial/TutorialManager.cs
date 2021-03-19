using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamestate;
using System.Linq;
using Photon.Pun;

public class TutorialManager : MonoBehaviour
{
    public GameObject movementObject; // active on start if player is driver
    public GameObject shootObject; // active on start if player is gunner
    public GameObject driftObject; // active 10 seconds after start if player is driver
    public GameObject selfDestructObject; // active if vehicle is (pretty much) stationary for 5 or more seconds

    float counter = 0f;

    GamestateTracker gamestateTracker;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();

        PlayerEntry player = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        if (player.role == (short)PlayerEntry.Role.Driver) {
            movementObject.SetActive(true);
            Invoke(nameof(ActivateDriftObject), 10f);
        }
        if (player.role == (short)PlayerEntry.Role.Gunner) {
            shootObject.SetActive(true);
        }
        player.Release();
    }

    void ActivateDriftObject() {
        driftObject.SetActive(true);
    }

    // Update is called once per frame
    void Update() {
        List<VehicleManager> vehicles = FindObjectsOfType<VehicleManager>().ToList();
        VehicleManager playerVehicle = null;
        PlayerEntry player = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        foreach (VehicleManager vehicle in vehicles) {
            if (player.teamId == vehicle.teamId) {
                playerVehicle = vehicle;
                break;
            }
        }
        player.Release();

        if (playerVehicle != null) {
            if (playerVehicle.GetComponent<Rigidbody>().velocity.sqrMagnitude < 5) {
                counter += Time.deltaTime;
            } else {
                counter = 0f;
            }
        }

        if (counter >= 5f) {
            selfDestructObject.SetActive(true);
        }
        
    }
}
