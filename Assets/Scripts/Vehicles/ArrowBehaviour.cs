using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Gamestate;
using Photon.Pun;

public class ArrowBehaviour : MonoBehaviour
{
    List<HotPotatoManager> hpms;
    public bool isEnabled;
    public float heightAboveCar = 7f;
    NetworkPlayerVehicle playerNPV;
    HotPotatoManager playerHPM;
    GamestateTracker gamestateTracker;
    MeshRenderer meshRenderer;

    public void ReadyUp() {
        hpms = FindObjectsOfType<HotPotatoManager>().ToList();
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        PlayerEntry player = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        List<NetworkPlayerVehicle> vehicles = FindObjectsOfType<NetworkPlayerVehicle>().ToList();
        foreach (NetworkPlayerVehicle vehicle in vehicles) {
            if (vehicle.teamId == player.teamId) {
                playerNPV = vehicle;
                playerHPM = vehicle.GetComponent<HotPotatoManager>();
                player.Release();
                break;
            }
        }

        isEnabled = true;
    }

    Transform GetPositionOfHotPotato() {
        foreach (HotPotatoManager hpm in hpms) {
            if (hpm.isPotato) {
                return hpm.gameObject.transform;
            }
        }
        PickupHotPotato potato = FindObjectOfType<PickupHotPotato>();
        return potato.gameObject.transform;
    }

    private void Update() {
        if (isEnabled) {
            if (playerHPM.isPotato) {
                meshRenderer.enabled = false;
            } else {
                meshRenderer.enabled = true;
                Transform potatoTransform = GetPositionOfHotPotato();
                transform.LookAt(potatoTransform);
                transform.position = playerNPV.gameObject.transform.position + new Vector3(0f, heightAboveCar, 0f);
            }
        }
    }
}
