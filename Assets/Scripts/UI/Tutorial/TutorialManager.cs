using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamestate;
using System.Linq;
using Photon.Pun;




public class TutorialManager : MonoBehaviour
{
    public List<bool> tutorials;

    float counter = 0f;

    GamestateTracker gamestateTracker;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();

        PlayerEntry player = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        // get 
        
        
        if (player.role == (short)PlayerEntry.Role.Driver) {
            tutorials[0] = true;
            Invoke(nameof(ActivateDriftObject), 30f);
        }
        if (player.role == (short)PlayerEntry.Role.Gunner) {
            tutorials[1] = true;
        }
        player.Release();
    }

    void ActivateDriftObject() {
        tutorials[2] = true;
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
            counter = 0f;
            tutorials[3] = true;
        }
        
    }

}
