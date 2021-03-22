using System;
using System.Collections;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class HitscanWeapon : Weapon
{
    public struct RaycastHitDetails
    {
        public Vector3 worldHitPoint;
        public Vector3 localHitPoint;
        public Transform hitTransform;
        public bool hasHealth;
        public bool validTarget;

        public RaycastHitDetails(Vector3 worldHit, Vector3 localHit, Transform hitT, bool healthExists, bool valid)
        {
            worldHitPoint = worldHit;
            localHitPoint = localHit;
            hitTransform = hitT;
            hasHealth = healthExists;
            validTarget = valid;
        }
    }

    [Header("Effect Settings")]
    public bool fireDummyProjectile = false;
    public GameObject dummyProjectile;
    public float dummyProjectileSpeed = 200f;
    public bool muzzleflashChildOfBarrel = false;
    
    [Header("Hitscan Settings")]

    
    public bool useTracerHitCorrection = true;
    public float hitscanRange = 10000f;
    protected Rigidbody parentRigidbody;
    // we should serialize this bool on change. when it is active and the photon view is not ours and optimisations are enabled, then do fire effects
    protected bool isFiring = false;
    
    [Header(("Hitscan Rapid Fire Optimisation Settings"))]
    public bool useRapidFireOptimisation = false;
    public float stopFireFxLoopThreshold = 0.2f;
    
    protected bool isRemotelyFiring = false;
    protected bool lastIsRemotelyFiring = false;

    
    private Collider[] colliders;
    
    
    
    protected void DoMuzzleFlashEffect()
    {
        // instantiate muzzleflash
        GameObject mFlash = Instantiate(muzzleflash, barrelEndMuzzleTransform.position, barrelEndMuzzleTransform.rotation);
        if (mFlash.GetComponent<AudioSource>() != null) mFlash.GetComponent<AudioSource>().volume = muzzleflashVolume;
        mFlash.transform.parent = barrelEndMuzzleTransform;
        Destroy(mFlash, 1f);
    }


    protected void SetIsRemotelyFiring(bool set)
    {
        if(set != isRemotelyFiring) weaponPhotonView.RPC(nameof(SetIsRemotelyFiring_RPC), RpcTarget.Others, set);
        if (set == true)
        {
            timeSinceLastFire = 0;
        }
        
        isRemotelyFiring = set;
    }
    [PunRPC]
    protected void SetIsRemotelyFiring_RPC(bool set)
    {
        isRemotelyFiring = set;
        Debug.Log(isRemotelyFiring && !weaponPhotonView.IsMine);
        if (set) timeSinceLastFire = 0;
        if (isRemotelyFiring && !weaponPhotonView.IsMine) StartCoroutine(RemoteFiringEffects());
    }

    protected IEnumerator RemoteFiringEffects()
    {
        while (isRemotelyFiring)
        {
            Debug.Log("RemoteFiringEffects");
            // fire dummy shots straight forward along barrel
            Vector3 targetPoint = barrelTransform.forward * 1000;
            RaycastHitDetails raycastTracerDetails = Fire_HitscanWeaponTracer(targetPoint);
            // fire effects based on raycastTracerDetails
            // ------------ local firing procedure:
            // if we hit, then fire a ray effect playing hitsound on hit
            if(raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget)FireHitscanRoundEffect(raycastTracerDetails.worldHitPoint);
            // if we miss, then fire a ray effect playing missound on hit
            else if(!raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget) FireHitscanRoundEffectMiss(raycastTracerDetails.worldHitPoint);
            // if valid target is null, then fire a ray effect with no impact
            else FireHitscanRoundEffectNoValidTarget(raycastTracerDetails.worldHitPoint);
            
            AnimatorSetTriggerNetwork(primaryFireAnimatorTriggerName);
            Debug.Log("RemoteFiringEffects done");
            yield return new WaitForSeconds(fireRate);
        }
    }

    protected void InstantiateImpactEffect(GameObject effect, Vector3 location, AudioClip clip ,float volume, float lifeTime)
    {
        GameObject impact = Instantiate(effect, location, Quaternion.identity);
        AudioSource a = impact.GetComponent<AudioSource>();
        a.clip = clip;
        a.volume = volume;
        a.Play();
        Destroy(a, lifeTime);
    }

    protected void InstantiateMuzzleFlash(GameObject effect, float lifetime, bool childOfMuzzle)
    {
        GameObject mFlash;
        mFlash = Instantiate(effect, barrelTransform.position, barrelTransform.rotation);
        if(childOfMuzzle)mFlash.transform.SetParent(barrelTransform);
        PlayAudioClipOneShot(weaponFireSound);
        Destroy(mFlash, lifetime);
    }


    protected new void Update()
    {
        
        // update loop for 
        base.Update();
        
        // if we have been firing more than the threshold
        if (timeSinceLastFire > stopFireFxLoopThreshold && isRemotelyFiring == true && weaponPhotonView.IsMine)
        {
            SetIsRemotelyFiring(false);
        }
        
        // synchronise remote firing for other clients
       /* if (lastIsRemotelyFiring != isRemotelyFiring && weaponPhotonView.IsMine)
        {
            weaponPhotonView.RPC(nameof(SetIsRemotelyFiring_RPC), RpcTarget.Others, isRemotelyFiring);
            lastIsRemotelyFiring = isRemotelyFiring;
        }*/
       
        
    }

    protected new void SetupWeapon()
    {
        base.SetupWeapon();
        colliders = transform.root.GetComponentsInChildren<Collider>();
    }

    public override void ActivateWeapon()
    {
        base.ActivateWeapon();
        SetupWeapon();
    }


    
    
    
    // Start is called before the first frame update
    // ---------------------- COPY THESE FUNCTIONS FOR EACH CHILD CLASS --------------------------------//
    // ---------------------- RPCs DO NOT INHERIT FROM PARENT ------------------------------------------//
    
    // method called by weaponController
    // we need a new version of this for every child class, otherwise the top level RPC will be called
    public override void Fire(Vector3 targetPoint)
    {
        if (CanFire() && gunnerPhotonView.IsMine)
        {
            targetPoint =
                CalculateFireDeviation(targetPoint,  projectileDeviationDegrees);
            
            currentCooldown = fireRate;
            UseAmmo(ammoPerShot);
            float distanceMultiplier = CalculateDamageMultiplierCurve(Vector3.Distance(barrelTransform.position, targetPoint));
            // define weapon damage details
            
            


            // now fire the original true round on the host's end
            // this is the shot that deals damage
            // this shot also returns the hit transform of what we hit
            // depending on if useTracerHitCorrection is enabled, call either of the FireHitscanRoundEffect rpcs
            RaycastHitDetails raycastTracerDetails = Fire_HitscanWeaponTracer(targetPoint);
            
            // if the raycast tracer details health field is not null, then damage em
            if (raycastTracerDetails.hasHealth)
            {
                WeaponDamageDetails weaponDamageDetails = new WeaponDamageDetails(myNickName, myPlayerId, myTeamId ,damageType, baseDamage*distanceMultiplier, raycastTracerDetails.localHitPoint);
                raycastTracerDetails.hitTransform.gameObject.GetComponentInParent<VehicleManager>().TakeDamage(weaponDamageDetails);
            }
            // do the fire effect on our end
            
            
            // ------------ local firing procedure:
            // if we hit, then fire a ray effect playing hitsound on hit
            if(raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget)FireHitscanRoundEffect(raycastTracerDetails.worldHitPoint);
            // if we miss, then fire a ray effect playing missound on hit
            else if(!raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget) FireHitscanRoundEffectMiss(raycastTracerDetails.worldHitPoint);
            // if valid target is null, then fire a ray effect with no impact
            else FireHitscanRoundEffectNoValidTarget(raycastTracerDetails.worldHitPoint);
            
            
            
            // -----------  remote firing procedure:
            // if optimisations are not enabled, for all other players:
            if (!useRapidFireOptimisation)
            {
                // if corrections are applied and we hit a target with health then fire a ray using FireHitscanRoundEffectCorrected, playing the hitsound on hit
                if (raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget && useTracerHitCorrection)
                {
                    int hitTeamId = raycastTracerDetails.hitTransform.GetComponentInParent<NetworkPlayerVehicle>().teamId;
                    weaponPhotonView.RPC(nameof(FireHitscanRoundEffectCorrected), RpcTarget.Others, raycastTracerDetails.localHitPoint, hitTeamId);
                }
                // if corrections are not applied and we hit a target, then fire a ray usig FireHitscanRoundEffect, playing hitsound on hit
                else if (raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget && !useTracerHitCorrection)
                {
                    weaponPhotonView.RPC(nameof(FireHitscanRoundEffect), RpcTarget.Others, raycastTracerDetails.worldHitPoint);
                }
                // if we hit something without health, then fire effect at hitpoint playing missound
                else if (!raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget)
                {
                    weaponPhotonView.RPC(nameof(FireHitscanRoundEffectMiss), RpcTarget.Others, raycastTracerDetails.worldHitPoint);
                }
                // if we hit no valid target (ie we hit the air), then just fire effect at hitpoint and instantiate no impact
                else
                {
                    weaponPhotonView.RPC(nameof(FireHitscanRoundEffectNoValidTarget), RpcTarget.Others, targetPoint);
                }
            }
 
            
            // if optimisations are enabled, then get the other clients to start firing pretend shots
            // this is useful for very rapid fire weapons where we don't really care where the bullets graphically go
            if (useRapidFireOptimisation)
            {
                // instantiate relevant hit impact on vehicle
                if (raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget && useTracerHitCorrection)
                {
                    int hitTeamId = raycastTracerDetails.hitTransform.GetComponentInParent<NetworkPlayerVehicle>().teamId;
                    weaponPhotonView.RPC(nameof(FireHitscanCorrectedImpact), RpcTarget.Others, raycastTracerDetails.localHitPoint, hitTeamId);
                }
                // this bool is synchronised via rpc on cooldown
                // the fire effects will fire the gun graphically in its current direction as long as it is active, causing no actual damage
                SetIsRemotelyFiring(true);
                
                // now that this is enabled, the guest will just fire and look after itself
            }
        }
    }
    


    protected void FireDummyProjectile(Vector3 target)
    {
        if (dummyProjectile != null&& fireDummyProjectile)
        {
            GameObject dummyProj = Instantiate(dummyProjectile, barrelTransform.position, barrelTransform.rotation);
            dummyProj.transform.LookAt(target);
            dummyProj.GetComponent<Rigidbody>().AddForce(dummyProj.transform.forward * dummyProjectileSpeed,
                ForceMode.VelocityChange);
            Destroy(dummyProj, 0.2f);
        }
    }
    
    
    [PunRPC]
    protected void FireHitscanRoundEffectNoValidTarget(Vector3 targetPoint)
    {
        AnimatorSetTriggerNetwork(primaryFireAnimatorTriggerName);
        // instantiate muzzle particle
        InstantiateMuzzleFlash(muzzleflash, 2f, muzzleflashChildOfBarrel);
        // instantiate tracer (if exists)
        FireDummyProjectile(targetPoint);
    }
    
    [PunRPC]
    protected void FireHitscanRoundEffectMiss(Vector3 targetPoint)
    {
        AnimatorSetTriggerNetwork(primaryFireAnimatorTriggerName);
        // instantiate muzzle particle
        InstantiateMuzzleFlash(muzzleflash, 2f, muzzleflashChildOfBarrel);
        // instantiate impact particle with miss sound effect
        InstantiateImpactEffect(missImpactParticle, targetPoint, impactParticleSoundMiss, missImpactParticleVolume, 2f);
        // instantiate tracer (if exists)
        FireDummyProjectile(targetPoint);
    }
    
    // handles the graphics for firing a hitscan round
    // round can leave a mini tracer for now
    [PunRPC]
    protected void FireHitscanRoundEffect(Vector3 targetPoint)
    {
        AnimatorSetTriggerNetwork(primaryFireAnimatorTriggerName);
        // instantiate muzzle particle
        InstantiateMuzzleFlash(muzzleflash, 2f, muzzleflashChildOfBarrel);
        
        // instantiate impact particle with hit sound effect
        InstantiateImpactEffect(imapactParticle, targetPoint, impactParticleSound, imapactParticleVolume, 2f);
        // instantiate tracer (if exists)
        FireDummyProjectile(targetPoint);
    }
    [PunRPC]
    protected void FireHitscanRoundEffectCorrected(Vector3 localTargetPoint, int hitTeamId)
    {
        
        AnimatorSetTriggerNetwork(primaryFireAnimatorTriggerName);
        // the vehicle transform tracker should keep a reference of all vehicle transforms in the game
        
        // get transform from team id
        Transform targetVehicle = _playerTransformTracker.GetVehicleTransformFromTeamId(hitTeamId);
        // convert local targetpoint into world point
        Vector3 worldPoint = targetVehicle.TransformPoint(localTargetPoint);
        // instantiate muzzle flash
        InstantiateMuzzleFlash(muzzleflash, 2f, muzzleflashChildOfBarrel);
        // instantiate impact at worldPoint
        InstantiateImpactEffect(imapactParticle, worldPoint, impactParticleSound, imapactParticleVolume, 2f);
        FireDummyProjectile(worldPoint);
        Debug.Log("FireHitscanRoundEffectCorrected: localTargetPoint " + localTargetPoint + " teamId " + hitTeamId + " worldPoint " + worldPoint);
    }
    [PunRPC]
    protected void FireHitscanCorrectedImpact(Vector3 localTargetPoint, int hitTeamId)
    {
        
        // the vehicle transform tracker should keep a reference of all vehicle transforms in the game
        // get transform from team id
        Transform targetVehicle = _playerTransformTracker.GetVehicleTransformFromTeamId(hitTeamId);
        // convert local targetpoint into world point
        Vector3 worldPoint = targetVehicle.TransformPoint(localTargetPoint);


        // instantiate impact at worldPoint
        InstantiateImpactEffect(imapactParticle, worldPoint, impactParticleSound, imapactParticleVolume, 2f);
    }



    // this is the true shot that we are firing at the enemy
    // fire the ray, determine the world, local
    protected  RaycastHitDetails Fire_HitscanWeaponTracer(Vector3 targetPoint)
    {
        
        // fire a ray from the barrel transform to the targetpoint
        Vector3 startPos = barrelTransform.position;
        
        Ray ray = new Ray(startPos, targetPoint - startPos); 
        //RaycastHit hit;
        RaycastHitDetails raycastHitDetails = FindClosestRaycastHitDetails(ray, targetPoint);
        // we now have our hit details, return them
        return raycastHitDetails;
    }
    
    

    protected RaycastHitDetails FindClosestRaycastHitDetails(Ray ray, Vector3 targetPoint)
    {
        RaycastHitDetails raycastHitDetails = new RaycastHitDetails(targetPoint, Vector3.zero, null, false, false);;
        
        RaycastHit[] hits = Physics.RaycastAll(ray);
        
        Transform closestHit = null;
        float distance = 0;
        Vector3 hitPoint = Vector3.zero;

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.root != this.transform && (closestHit == null || hit.distance < distance) && !colliders.Contains(hit.collider))
            {
                // We have hit something that is:
                // a) not us
                // b) the first thing we hit (that is not us)
                // c) or, if not b, is at least closer than the previous closest thing

                closestHit = hit.transform;
                distance = hit.distance;
                hitPoint = hit.point;
                
                // get local hitpoint
                Vector3 localHitPoint = closestHit.root.InverseTransformPoint(hitPoint);
                // the health script exists
                if (hit.transform.root.GetComponent<VehicleManager>() != null)
                {
                    raycastHitDetails = new RaycastHitDetails(hitPoint,localHitPoint,closestHit,true,true );
                }
                else
                {
                    raycastHitDetails = new RaycastHitDetails(hitPoint,localHitPoint,closestHit,false,true );
                }

            }
        }
        

        // closestHit is now either still null (i.e. we hit nothing) OR it contains the closest thing that is a valid thing to hit

        return raycastHitDetails;

    }
}
