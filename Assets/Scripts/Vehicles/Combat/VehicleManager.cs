using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    GamestateTracker gamestateTracker;
    NetworkManager networkManager;
    PhotonView driverPhotonView;
    Rigidbody rb;
    InterfaceCarDrive icd;
    InputDriver inputDriver;
    IDrivable carDriver;
    public int teamId;
    public float health = 100f;
    float maxHealth;
    public GameObject temporaryDeathExplosion;
    PhotonView gamestateTrackerPhotonView;
    bool isDead = false;
    
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
        driverPhotonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update() { 
        
    }

    [PunRPC]
    void TakeDamage_RPC(string weaponDetailsJson)
    {
        Weapon.WeaponDamageDetails weaponDamageDetails =
            JsonUtility.FromJson<Weapon.WeaponDamageDetails>(weaponDetailsJson);
        lastHitDetails = weaponDamageDetails;
        float amount = weaponDamageDetails.damage;
    //    Debug.Log("Damage taken by: " + weaponDamageDetails.sourcePlayerNickName);
        if (health > 0) {
            health -= amount;
            if (health <= 0&&!isDead && driverPhotonView.IsMine)
            {
                // die is only called once, by the driver
                isDead = true;
                Die(true, true);
                // do death effects for all other players
                
                // TODO- update to take damage type parameter
                driverPhotonView.RPC(nameof(PlayDeathEffects_RPC), RpcTarget.All);
                
            }
        }
    }

    [PunRPC]
    void TakeAnonymousDamage_RPC(float amount)
    {
        Debug.Log("Taken anonymous damage");
        if (health > 0) {
            health -= amount;
            Debug.Log("passed health check");
            if (health <= 0&&!isDead && driverPhotonView.IsMine)
            {
                Debug.Log("passed death check");
                // die is only called once, by the driver
                isDead = true;
                Die(true, false);
                // do death effects for all other players

                // TODO- update to take damage type parameter
                driverPhotonView.RPC(nameof(PlayDeathEffects_RPC), RpcTarget.All);
            }
        }
    }

    public void TakeDamage(Weapon.WeaponDamageDetails hitDetails)
    {
        // call take damage on everyone else's instance of the game
        string hitDetailsJson = JsonUtility.ToJson(hitDetails);
        driverPhotonView.RPC(nameof(TakeDamage_RPC), RpcTarget.All, hitDetailsJson);
    }

    // overloaded method that doesn't care about assigning a kill
    public void TakeDamage(float amount)
    {
        driverPhotonView.RPC(nameof(TakeAnonymousDamage_RPC), RpcTarget.All, amount);
    }

    void PlayDeathTrailEffects(bool childExplosion)
    {
        if (temporaryDeathExplosion != null)
        {
            GameObject temporaryDeathExplosionInstance = Instantiate(temporaryDeathExplosion, transform.position, transform.rotation);
            if(childExplosion) temporaryDeathExplosionInstance.transform.SetParent(transform);
        }
    }

    
    // Die is a LOCAL function that is only called by the driver when they get dead.
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
            Debug.Log("Kill earned by: " + lastHitDetails.sourceTeamId + " team");
            GamestateTracker.TeamDetails theirRecord = gamestateTracker.getTeamDetails(lastHitDetails.sourceTeamId);
            theirRecord.kills += 1;
            gamestateTrackerPhotonView.RPC(nameof(GamestateTracker.UpdateTeamWithNewRecord), RpcTarget.All,
                lastHitDetails.sourceTeamId, JsonUtility.ToJson(theirRecord));
        }



        networkManager.CallRespawnVehicle(5f, teamId);
        

    }

    [PunRPC]
    void PlayDeathEffects_RPC()
    {
        PlayDeathTrailEffects(true);
        inputDriver.enabled = false;
        rb.drag = 0.75f;
        rb.angularDrag = 0.75f;
        StartCoroutine(stopControls(2.95f));
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
        
        
        // call network delete on driver instance
        if(driverPhotonView.IsMine)PhotonNetwork.Destroy(gameObject);

        
    }

}
