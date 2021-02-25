using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    [Serializable]
    public struct CollisionArea {
        public Vector3 rotationEuler;

        [HideInInspector]
        public Quaternion rotation;
        public float width;
        public float height;
        public float collisionResistance;
    }

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
    public List<CollisionArea> collisionAreas;
    private float deathForce = Mathf.Pow(10, 6.65f);
    private float baseCollisionResistance = 1;
    private Weapon.WeaponDamageDetails _rammingDetails;
    public Weapon.WeaponDamageDetails rammingDetails {
        get {
            if (_rammingDetails.sourcePlayerNickName == null) {
                NetworkPlayerVehicle npv = GetComponent<NetworkPlayerVehicle>();
                _rammingDetails.sourcePlayerNickName = npv.GetDriverNickName();
                _rammingDetails.sourcePlayerId = npv.GetDriverID();
                _rammingDetails.sourceTeamId = npv.teamId;
            }
            return _rammingDetails;
        }
    }
    public float defaultCollisionResistance = 1;
    public float environmentCollisionResistance = 1;
    
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

        baseCollisionResistance = deathForce / maxHealth;

        _rammingDetails = new Weapon.WeaponDamageDetails(null, 0, 0, Weapon.DamageType.ramming, 0);
    }

    void OnDrawGizmos() {
        Quaternion originalRotation = transform.rotation;

        for (int i = 0; i < collisionAreas.Count; i++) {
            CollisionArea collisionArea = collisionAreas[i];
            collisionArea.rotation.eulerAngles = collisionArea.rotationEuler;
            transform.rotation = collisionArea.rotation;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawFrustum(Vector3.zero, collisionArea.height, 3, 0, collisionArea.width / collisionArea.height);
        }

        transform.rotation = originalRotation;
    }

    void OnCollisionEnter(Collision collision) {
        Vector3 collisionNormal = collision.GetContact(0).normal;
        Vector3 collisionForce = collision.impulse;
        if (Vector3.Dot(collisionForce, collisionNormal) < 0) collisionForce = -collisionForce;
        collisionForce /= Time.fixedDeltaTime;
        collisionForce = transform.InverseTransformDirection(collisionForce);

        VehicleManager otherVehicleManager = collision.gameObject.GetComponent<VehicleManager>();

        Vector3 contactPoint = transform.InverseTransformPoint(collision.GetContact(0).point);
        float damage = CalculateCollisionDamage(collisionForce, contactPoint, otherVehicleManager != null);

        if (otherVehicleManager != null) {
            Weapon.WeaponDamageDetails rammingDetails = otherVehicleManager.rammingDetails;
            rammingDetails.damage = damage;
            TakeDamage(rammingDetails);
        }
        else {
            TakeDamage(damage);
        }
    }

    private float CalculateCollisionDamage(Vector3 collisionForce, Vector3 collisionDirection, bool hitVehicle) {
        float collisionResistance = 1;

        foreach (CollisionArea collisionArea in collisionAreas) {
            Vector3 verticalComponent = Vector3.ProjectOnPlane(-collisionDirection, collisionArea.rotation * Vector3.right).normalized;
            Vector3 horizontalComponent = Vector3.ProjectOnPlane(-collisionDirection, collisionArea.rotation * Vector3.up).normalized;
            Vector3 areaCentre = collisionArea.rotation * Vector3.forward;
            if (Vector3.Dot(areaCentre, verticalComponent) < Mathf.Cos(collisionArea.height / 2) &&
                Vector3.Dot(areaCentre, horizontalComponent) < Mathf.Cos(collisionArea.width / 2)) {

                collisionResistance = collisionArea.collisionResistance;
                break;
            }
        }

        float reducedForce = collisionForce.magnitude / baseCollisionResistance;
        if (!hitVehicle) reducedForce /= environmentCollisionResistance;
        reducedForce /= collisionResistance;

        return reducedForce;
    }

    [PunRPC]
    public void SetTeamId_RPC(int newId) {
        teamId = newId;
    }

    [PunRPC]
    void TakeDamage_RPC(string weaponDetailsJson)
    {
        Weapon.WeaponDamageDetails weaponDamageDetails =
            JsonUtility.FromJson<Weapon.WeaponDamageDetails>(weaponDetailsJson);
        lastHitDetails = weaponDamageDetails;
        float amount = weaponDamageDetails.damage;
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
        if (health > 0) {
            health -= amount;
            if (health <= 0&&!isDead && driverPhotonView.IsMine)
            {
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
        health = 0;
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
        if (driverPhotonView.IsMine) PhotonNetwork.Destroy(gameObject);

        
    }

}
