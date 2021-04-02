using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;
using Gamestate;
using Photon.Realtime;


public class VehicleHealthManager : CollidableHealthManager
{
    public ParticleSystem smokeL;
    public ParticleSystem smokeM;
    public ParticleSystem smokeH;

    // car stuff
    protected Weapon.WeaponDamageDetails _rammingDetails;

    public GameObject tutorials;

    protected PlayerTransformTracker playerTransformTracker;
    
    float defaultDrag = 0.15f;
    float defaultAngularDrag = 0.2f;
    Vector3 defaultCOM;   
    
    Rigidbody rb;
    InterfaceCarDrive icd;
    InputDriver inputDriver;
    IDrivable carDriver;
    NetworkPlayerVehicle npv;
    public int teamId {
        get {
            return npv.teamId;
        }
        set {
            npv.teamId = value;
        }
    }
    
    public GameObject temporaryDeathExplosion;

    public Weapon.WeaponDamageDetails rammingDetails {
        get {
            if (_rammingDetails.sourcePlayerNickName == null) {
                
                _rammingDetails.sourcePlayerNickName = npv.GetDriverNickName();
                _rammingDetails.sourcePlayerId = npv.GetDriverID();
                _rammingDetails.sourceTeamId = npv.teamId;
            }
            return _rammingDetails;
        }
    }
    
    public void SetupVehicleManager() {
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        gamestateTrackerPhotonView = gamestateTracker.GetComponent<PhotonView>();
        networkManager = FindObjectOfType<NetworkManager>();
        maxHealth = health;
        rb = GetComponent<Rigidbody>();
        icd = GetComponent<InterfaceCarDrive>();
        carDriver = icd.GetComponent<IDrivable>();
        inputDriver = GetComponent<InputDriver>();
        myPhotonView = GetComponent<PhotonView>();
        npv = GetComponent<NetworkPlayerVehicle>();

        

        _rammingDetails = new Weapon.WeaponDamageDetails(null, 0, 0, Weapon.DamageType.ramming, 0, Vector3.zero);

        for (int i = 0; i < collisionAreas.Count; i++) {
            CollisionArea collisionArea = collisionAreas[i];
            collisionArea.rotation.eulerAngles = collisionArea.rotationEuler;
            collisionAreas[i] = collisionArea;
        }

        defaultDrag = rb.drag;
        defaultAngularDrag = rb.angularDrag;
        defaultCOM = GetComponent<COMDropper>().Shift;
        playerTransformTracker = FindObjectOfType<PlayerTransformTracker>();

        PlayerEntry player = gamestateTracker.players.Get((short)PhotonNetwork.LocalPlayer.ActorNumber);
        if (player.teamId == teamId) tutorials.SetActive(true);
        else tutorials.SetActive(false);
    }

    public override void SetupHealthManager()
    {
        base.SetupHealthManager();
        SetupVehicleManager();
    }

    void OnDrawGizmos() {
        Quaternion originalRotation = transform.rotation;

        for (int i = 0; i < collisionAreas.Count; i++) {
            CollisionArea collisionArea = collisionAreas[i];
            if (collisionArea.show) {
                collisionArea.rotation.eulerAngles = collisionArea.rotationEuler;
                transform.rotation *= collisionArea.rotation;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawFrustum(Vector3.zero, collisionArea.height, 3, 0, collisionArea.width / collisionArea.height);

                transform.rotation = originalRotation;
            }
        }
    }

    [PunRPC]
    protected override void TakeDamage_RPC(string weaponDetailsJson)
    {
        Weapon.WeaponDamageDetails weaponDamageDetails =
            JsonUtility.FromJson<Weapon.WeaponDamageDetails>(weaponDetailsJson);
        lastHitDetails = weaponDamageDetails;
        float amount = weaponDamageDetails.damage;
        if (health > 0) {
            health -= amount;
            if (health > maxHealth) health = maxHealth;
            smokeL.Stop();
            smokeM.Stop();
            smokeH.Stop();
            if (health < maxHealth * 0.6) {
                smokeL.Play();
                smokeM.Stop();
                smokeH.Stop();
            }
            if (health < maxHealth * 0.4) {
                smokeL.Stop();
                smokeM.Play();
                smokeH.Stop();

            }
            if (health < maxHealth * 0.2) {
                smokeL.Stop();
                smokeM.Play();
                smokeH.Play();
            }
            if (health <= 0 && !isDead && myPhotonView.IsMine)
            {
                // die is only called once, by the driver
                isDead = true;
                Die(true, true);
                // do death effects for all other players
                
                // TODO- update to take damage type parameter
                myPhotonView.RPC(nameof(PlayDeathEffects_RPC), RpcTarget.All); 
                
            }
        }
    }

    [PunRPC]
    protected override void TakeAnonymousDamage_RPC(float amount)
    {
        if (health > 0) {
            health -= amount;
            if (health > maxHealth) health = maxHealth;
            smokeL.Stop();
            smokeM.Stop();
            smokeH.Stop();
            if (health < maxHealth * 0.6) {
                smokeL.Play();
                smokeM.Stop();
                smokeH.Stop();
            }
            if (health < maxHealth * 0.4) {
                smokeL.Stop();
                smokeM.Play();
                smokeH.Stop();

            }
            if (health < maxHealth * 0.2) {
                smokeL.Stop();
                smokeM.Play();
                smokeH.Play();
            }
            if (health <= 0&&!isDead && myPhotonView.IsMine)
            {
                // die is only called once, by the driver
                isDead = true;
                Die(true, false);
                // do death effects for all other players

                // TODO- update to take damage type parameter
                myPhotonView.RPC(nameof(PlayDeathEffects_RPC), RpcTarget.All);
            }
        }
    }

    protected void ChargeDriverAbility(Weapon.WeaponDamageDetails hitDetails)
    {
        int sourceTeamId = hitDetails.sourceTeamId;
        Transform sourceTransform = playerTransformTracker.GetVehicleTransformFromTeamId(sourceTeamId);
        // now add damage dealt back to the source transform driver ability manager
        GunnerWeaponManager gunnerWeaponManager = sourceTransform.GetComponentInChildren<GunnerWeaponManager>();
        
        // adjust damage dealth modifier
        gunnerWeaponManager.UpdateDamageDealt(hitDetails);

    }

    public override void TakeDamage(Weapon.WeaponDamageDetails hitDetails)
    {

        ChargeDriverAbility(hitDetails);
        
        
        // call take damage on everyone else's instance of the game
        string hitDetailsJson = JsonUtility.ToJson(hitDetails);
        
        myPhotonView.RPC(nameof(TakeDamage_RPC), RpcTarget.All, hitDetailsJson);
    }

    // overloaded method that doesn't care about assigning a kill
    public override void TakeDamage(float amount)
    {
        myPhotonView.RPC(nameof(TakeAnonymousDamage_RPC), RpcTarget.All, amount);
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
    protected void Die(bool updateDeath, bool updateKill) {
        // Update gamestate
        TeamEntry team = gamestateTracker.teams.Get((short)teamId);
        myPhotonView.RPC(nameof(SetGunnerHealth_RPC), RpcTarget.All, 0f);
        team.Release();
        // update my deaths
        if (updateDeath)
        {
            /*GamestateTracker.TeamDetails myRecord = gamestateTracker.getTeamDetails(teamId);
            myRecord.deaths += 1;
            myRecord.isDead = true;
            gamestateTrackerPhotonView.RPC(nameof(GamestateTracker.UpdateTeamWithNewRecord), RpcTarget.All, teamId,
                JsonUtility.ToJson(myRecord));*/

            TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);
            teamEntry.deaths += 1;
            teamEntry.isDead = true;
            teamEntry.Increment();
        }

        if (updateKill)
        {
            // update their kills
            /*GamestateTracker.TeamDetails theirRecord = gamestateTracker.getTeamDetails(lastHitDetails.sourceTeamId);
            theirRecord.kills += 1;
            gamestateTrackerPhotonView.RPC(nameof(GamestateTracker.UpdateTeamWithNewRecord), RpcTarget.All,
                lastHitDetails.sourceTeamId, JsonUtility.ToJson(theirRecord));*/
            
            TeamEntry teamEntry = gamestateTracker.teams.Get((short)lastHitDetails.sourceTeamId);
            teamEntry.kills += 1;
            teamEntry.Increment();
        }



        networkManager.CallRespawnVehicle(5f, teamId);
        

    }

    [PunRPC]
    protected override void PlayDeathEffects_RPC()
    {
        PlayDeathTrailEffects(true);
        inputDriver.enabled = false;
        rb.drag = 0.75f;
        rb.angularDrag = 0.75f;
        float x, y, z;
        x = Random.Range(-0.5f, 0.5f);
        if (x < 0) {
            x -= 1.5f;
        } else {
            x += 1.5f;
        }
        y = 0.9f;
        z = Random.Range(0, 2f);
        smokeM.Play();
        smokeH.Play();

        Vector3 explodePos = new Vector3(x, y, z);
        Vector3 newCom = new Vector3(0, 1.3f, 0);
        rb.centerOfMass = newCom;
        rb.angularDrag = 0.1f;
        rb.AddForce(explodePos*rb.mass * 10f, ForceMode.Impulse);
        rb.AddTorque(explodePos * rb.mass * 4f, ForceMode.Impulse);
        StartCoroutine(stopControls(1.7f));
    }

    IEnumerator stopControls(float time) {
        // Wait for respawn time
        carDriver.StopAccellerate();
        carDriver.StopBrake();
        carDriver.StopSteer();
        yield return new WaitForSecondsRealtime(time);

        /*MonoBehaviour[] childBehaviours = GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour childBehaviour in childBehaviours)
        {
            childBehaviour.enabled = false;
        }*/
        PlayDeathTrailEffects(false);
        
        
        // call network delete on driver instance
        //if (myPhotonView.IsMine) PhotonNetwork.Destroy(gameObject);*/

        
    }

    // [PunRPC]
    // protected new void PlayDamageSoundNetwork(float damage)
    // {
    //     base.PlayDamageSoundNetwork(damage);
    // }

    [PunRPC]
    void SetGunnerHealth_RPC(float value) {
        health = value;
    }

    [PunRPC]
    void ResetMesh_RPC() {
        GetComponent<Squishing>().ResetMesh();
    }

    public void ResetProperties() {
        Debug.Log("reset properties");
        isDead = false;
        TeamEntry team = gamestateTracker.teams.Get((short)teamId);
        myPhotonView.RPC(nameof(SetGunnerHealth_RPC), RpcTarget.All, maxHealth);
        team.Release();
        GunnerWeaponManager gunnerWeaponManager = GetComponentInChildren<GunnerWeaponManager>();
        gunnerWeaponManager.Reset();

        DriverAbilityManager driverAbilityManager = GetComponent<DriverAbilityManager>();
        driverAbilityManager.Reset();

        smokeL.Stop();
        smokeM.Stop();
        smokeH.Stop();

        rb.drag = defaultDrag;
        rb.angularDrag = defaultAngularDrag;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isDead = false;
        rb.centerOfMass = defaultCOM;
        TeamEntry teamEntry = gamestateTracker.teams.Get((short)teamId);
        teamEntry.isDead = false;
        teamEntry.Increment();
        myPhotonView.RPC(nameof(ResetMesh_RPC), RpcTarget.AllBuffered);
        GetComponentInChildren<DriverCinematicCam>().ResetCam();
    }

}
