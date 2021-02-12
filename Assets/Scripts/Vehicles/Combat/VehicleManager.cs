using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    GamestateTracker gamestateTracker;
    NetworkManager networkManager;
    Rigidbody rb;
    InterfaceCarDrive icd;
    InputDriver inputDriver;
    IDrivable carDriver;
    public int teamId;
    public float health = 100f;
    float maxHealth;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        networkManager = FindObjectOfType<NetworkManager>();
        maxHealth = health;
        rb = GetComponent<Rigidbody>();
        icd = GetComponent<InterfaceCarDrive>();
        carDriver = icd.GetComponent<IDrivable>();
        inputDriver = GetComponent<InputDriver>();
    }

    // Update is called once per frame
    void Update() { 
        
    }

    public void takeDamage(float amount) {
        if (health > 0) {
            health -= amount;
            if (health <= 0) {
                die();
            }
        }
    }

    void die() {
        // Update gamestate
        GamestateTracker.TeamDetails record = gamestateTracker.getTeamDetails(teamId);
        record.deaths += 1;
        record.isDead = true;
        gamestateTracker.UpdateTeamWithNewRecord(teamId, JsonUtility.ToJson(record));

        StartCoroutine(networkManager.respawnVehicle(3f, teamId));
        inputDriver.enabled = false;
        rb.drag = 0.75f;
        rb.angularDrag = 0.75f;
        StartCoroutine(stopControls(3f));
    }

    IEnumerator stopControls(float time) {
        // Wait for respawn time
        yield return new WaitForSecondsRealtime(time);
        carDriver.StopAccellerate();
        carDriver.StopBrake();
        carDriver.StopSteer();
    }

}
