using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleStats : MonoBehaviour
{
    GamestateTracker gamestateTracker;
    public int teamId;
    public float health = 100f;
    float maxHealth;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        maxHealth = health;
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
        GamestateTracker.TeamDetails record = gamestateTracker.getTeamDetails(teamId);
        record.deaths += 1;
        record.isDead = true;
        gamestateTracker.UpdateTeamWithNewRecord(teamId, JsonUtility.ToJson(record));
        StartCoroutine(respawn(3f, transform));
    }

    IEnumerator respawn(float time, Transform respawnLocation) {
        yield return new WaitForSecondsRealtime(time);
        GamestateTracker.TeamDetails record = gamestateTracker.getTeamDetails(teamId);
        record.isDead = false;
        health = maxHealth;
        transform.position = respawnLocation.position;
        transform.rotation = respawnLocation.rotation;
        gamestateTracker.UpdateTeamWithNewRecord(teamId, JsonUtility.ToJson(record));
        Debug.Log("Respawned!!!");
    }

}
