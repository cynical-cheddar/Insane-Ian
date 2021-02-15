using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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
    public GameObject temporaryDeathExplosion;
    PhotonView gamestateTrackerPhotonView;
    
    Weapon.WeaponDamageDetails lastHitDetails;

    // Start is called before the first frame update
    void Start() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        gamestateTrackerPhotonView = gamestateTracker.GetComponent<PhotonView>();
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

    public void TakeDamage(Weapon.WeaponDamageDetails hitDetails)
    {
        lastHitDetails = hitDetails;
        float amount = hitDetails.damage;
        Debug.Log("Damage taken by: " + hitDetails.sourcePlayerNickName);
        if (health > 0) {
            health -= amount;
            if (health <= 0) {
                Die(true, true);
            }
        }
    }

    // overloaded method that doesn't care about assigning a kill
    public void TakeDamage(float amount)
    {
        if (health > 0) {
            health -= amount;
            if (health <= 0) {
                // don't take kill
                Die(true, false);
            }
        }
    }

    void PlayDeathTrailEffects(bool childExplosion)
    {
        if (temporaryDeathExplosion != null)
        {
            GameObject temporaryDeathExplosionInstance = Instantiate(temporaryDeathExplosion, transform.position, transform.rotation);
            if(childExplosion) temporaryDeathExplosionInstance.transform.SetParent(transform);
        }
    }

    void Die(bool updateDeath, bool updateKill) {
        // Update gamestate
        
        // update my deaths
        if (updateDeath)
        {
            GamestateTracker.TeamDetails myRecord = gamestateTracker.getTeamDetails(teamId);
            myRecord.deaths += 1;
            myRecord.isDead = true;
            gamestateTrackerPhotonView.RPC(nameof(GamestateTracker.UpdateTeamWithNewRecord), RpcTarget.All, teamId,
                JsonUtility.ToJson(myRecord));
        }

        if (updateKill)
        {
            // update their kills
            GamestateTracker.TeamDetails theirRecord = gamestateTracker.getTeamDetails(lastHitDetails.sourcePlayerId);
            theirRecord.kills += 1;
            gamestateTrackerPhotonView.RPC(nameof(GamestateTracker.UpdateTeamWithNewRecord), RpcTarget.All,
                lastHitDetails.sourcePlayerId, JsonUtility.ToJson(theirRecord));
        }

        PlayDeathTrailEffects(true);

        StartCoroutine(networkManager.RespawnVehicle(3f, teamId));
        inputDriver.enabled = false;
        rb.drag = 0.75f;
        rb.angularDrag = 0.75f;
        StartCoroutine(stopControls(2.9f));
    }

    IEnumerator stopControls(float time) {
        // Wait for respawn time
        carDriver.StopAccellerate();
        carDriver.StopBrake();
        carDriver.StopSteer();
        yield return new WaitForSecondsRealtime(time);
        

        MonoBehaviour[] childBehaviours = GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour childBehaviour in childBehaviours)
        {
            childBehaviour.enabled = false;
        }
        PlayDeathTrailEffects(false);
        PhotonNetwork.Destroy(gameObject);

        
    }

}
