using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhysX;

public class LightningWeapon : Weapon
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


    public bool muzzleflashChildOfBarrel = false;
    
    [Header("Hitscan Settings")]
    public bool useTracerHitCorrection = true;

    [Header("Effects")]
    public GameObject lightningPrefab;

    public float lightningBoltDuration = 0.25f;
    
    private Collider[] colliders;

    protected GameObject endObject;
    protected GameObject startObject;
    
    
    protected void DoMuzzleFlashEffect()
    {
        // instantiate muzzleflash
        GameObject mFlash = Instantiate(muzzleflash, barrelEndMuzzleTransform.position, barrelEndMuzzleTransform.rotation);
        if (mFlash.GetComponent<AudioSource>() != null) mFlash.GetComponent<AudioSource>().volume = muzzleflashVolume;
        mFlash.transform.parent = barrelEndMuzzleTransform;
        Destroy(mFlash, 1f);
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

    protected void InstantiateLightning(Vector3 location, float lifetime)
    {
        GameObject lightning1 = Instantiate(lightningPrefab, barrelTransform.position, Quaternion.identity);
        LightningBoltScript lbs = lightning1.GetComponent<LightningBoltScript>();
        endObject.transform.position = location;
        lbs.EndObject = endObject;
        lbs.StartObject = startObject;
        lbs.FadeAway(lifetime);
        
        
    }


    protected new void Update()
    {
        
        // update loop for 
        base.Update();
        
       
        
    }

    protected new void SetupWeapon()
    {
        base.SetupWeapon();
        colliders = transform.root.GetComponentsInChildren<Collider>();
        endObject = Instantiate(new GameObject(), transform.position, Quaternion.identity);
        endObject.name = "endObject";
        
        startObject = Instantiate(new GameObject(), barrelTransform.position, Quaternion.identity);
        startObject.transform.parent = barrelTransform;
        startObject.name = "startObject";
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
            ShakeCameras(cameraShakeAmplitude, cameraShakeDuration);
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
                raycastTracerDetails.hitTransform.gameObject.GetComponentInParent<VehicleHealthManager>().TakeDamage(weaponDamageDetails);
            }
            // do the fire effect on our end
            
            
            // ------------ local firing procedure:
            // if we hit, then fire a ray effect playing hitsound on hit
            if(raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget)FireHitscanRoundEffect(raycastTracerDetails.worldHitPoint);
            // if we miss, then fire a ray effect playing missound on hit
            else if(!raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget) FireHitscanRoundEffectMiss(raycastTracerDetails.worldHitPoint);
            // if valid target is null, then fire a ray effect with no impact
            else FireHitscanRoundEffectNoValidTarget(raycastTracerDetails.worldHitPoint);
            // do camera shake
            ShakeCameras(cameraShakeAmplitude, cameraShakeDuration);
            shakeTimerCur = 0;
            
            
            
            // -----------  remote firing procedure:
            // if optimisations are not enabled, for all other players:

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
    }
    



    
    
    [PunRPC]
    protected void FireHitscanRoundEffectNoValidTarget(Vector3 targetPoint)
    {
        AnimatorSetTriggerNetwork(primaryFireAnimatorTriggerName);
        // instantiate muzzle particle
        InstantiateMuzzleFlash(muzzleflash, 2f, muzzleflashChildOfBarrel);
        
        InstantiateLightning(targetPoint, lightningBoltDuration);
    }
    
    [PunRPC]
    protected void FireHitscanRoundEffectMiss(Vector3 targetPoint)
    {
        AnimatorSetTriggerNetwork(primaryFireAnimatorTriggerName);
        // instantiate muzzle particle
        InstantiateMuzzleFlash(muzzleflash, 2f, muzzleflashChildOfBarrel);
        // instantiate impact particle with miss sound effect
        InstantiateImpactEffect(missImpactParticle, targetPoint, impactParticleSoundMiss, missImpactParticleVolume, 2f);
        
        InstantiateLightning(targetPoint, lightningBoltDuration);
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

        InstantiateLightning(targetPoint, lightningBoltDuration);
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
        
        InstantiateLightning(worldPoint, lightningBoltDuration);
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
        
        //RaycastHit[] hits = Physics.RaycastAll(ray);
        
       // if (PhysXRaycast.Fire(sensorStartPos, transform.forward, hit, sensorLength, sensorLayerMask, myRb.vehicleId)) {

        Transform closestHit = null;
        float distance = 0;
        Vector3 hitPoint = Vector3.zero;

        PhysXRaycastHit hitPhysX = PhysXRaycast.GetRaycastHit();
         if (PhysXRaycast.Fire(ray.origin, ray.direction, hitPhysX, 999, raycastLayers, rigidbodyVehicleId)){
             closestHit = hitPhysX.transform;
                distance = hitPhysX.distance;
                hitPoint = hitPhysX.point;
                
                // get local hitpoint
                Vector3 localHitPoint = closestHit.root.InverseTransformPoint(hitPoint);
                // the health script exists
                if (hitPhysX.transform.root.GetComponent<VehicleHealthManager>() != null)
                {
                    raycastHitDetails = new RaycastHitDetails(hitPoint,localHitPoint,closestHit,true,true );
                }
                else
                {
                    raycastHitDetails = new RaycastHitDetails(hitPoint,localHitPoint, closestHit,false,true );
                }
         }
         PhysXRaycast.ReleaseRaycastHit(hitPhysX);

        return raycastHitDetails;
    

    }
}
