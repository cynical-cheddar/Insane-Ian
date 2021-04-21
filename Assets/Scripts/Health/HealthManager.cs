using System;
using System.Collections;
using System.Collections.Generic;
using Gamestate;
using Photon.Pun;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    protected GamestateTracker gamestateTracker;
    protected PhotonView gamestateTrackerPhotonView;
    protected NetworkManager networkManager;
    protected PhotonView myPhotonView;
    
    public float health = 100f;
    protected float maxHealth = 100f;
    
    protected Weapon.WeaponDamageDetails lastHitDetails;
    
    protected bool isDead = false;

    public float rammingDamageResistance = 1f;
    
    
    public bool isAtFullHealth {
        get {
            return health == maxHealth;
        }
    }

    public float scaledHealth {
        get {
            return health / maxHealth;
        }
    }


    protected void Start()
    {
        SetupHealthManager();
    }
    public virtual void SetupHealthManager()
    {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        gamestateTrackerPhotonView = gamestateTracker.GetComponent<PhotonView>();
        networkManager = FindObjectOfType<NetworkManager>();
        maxHealth = health;
        myPhotonView = GetComponent<PhotonView>();
    }
    
    
    public virtual void TakeDamage(Weapon.WeaponDamageDetails hitDetails)
    {
        // call take damage on everyone else's instance of the game
        
        
        string hitDetailsJson = JsonUtility.ToJson(hitDetails);
        
        myPhotonView.RPC(nameof(TakeDamage_RPC), RpcTarget.All, hitDetailsJson);
    }

    // overloaded method that doesn't care about assigning a kill
    public virtual void TakeDamage(float amount)
    {
        myPhotonView.RPC(nameof(TakeAnonymousDamage_RPC), RpcTarget.All, amount);
    }

    public virtual void HealObject(float amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
        myPhotonView.RPC(nameof(SetHealth_RPC), RpcTarget.All, health);
    }

    [PunRPC]
    protected void SetHealth_RPC(float amt)
    {
        health = amt;
    }
    
    
    [PunRPC]
    protected virtual void TakeDamage_RPC(string weaponDetailsJson) {
        Weapon.WeaponDamageDetails weaponDamageDetails = JsonUtility.FromJson<Weapon.WeaponDamageDetails>(weaponDetailsJson);
        lastHitDetails = weaponDamageDetails;
        
        float amount = weaponDamageDetails.damage;
        if (health > 0) {
            health -= amount;

            if (health <= 0&&!isDead && myPhotonView.IsMine) {
                // die is only called once, by the driver
                Die();
            }
        }
    }

    [PunRPC]
    protected virtual void TakeAnonymousDamage_RPC(float amount)
    {
        if (health > 0) {
            health -= amount;

            if (health <= 0&&!isDead && myPhotonView.IsMine) {
                // die is only called once, by the driver    
                Die();
            }
        }
    }
    
    // Die is a LOCAL function that is only called by the owner when they get dead.
    protected virtual void Die() {
        health = 0;
        isDead = true;
        myPhotonView.RPC(nameof(PlayDeathEffects_RPC), RpcTarget.All);
    }
    
    [PunRPC]
    protected virtual void PlayDeathEffects_RPC() {
        PhotonNetwork.Destroy(gameObject);
    }
}
