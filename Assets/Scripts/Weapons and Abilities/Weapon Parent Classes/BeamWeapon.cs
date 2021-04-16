using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class BeamWeapon : Weapon
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
    
    
    [Serializable]
    public struct BeamHit
    {
        public bool validHit;
        public Vector3 worldHitPoint;
        public bool hasHitPlayer;
        public Vector3 localHitpoint;
    
        public bool active;
        public int hitTeamId;
        public Transform hitTransform;
        public BeamHit(bool valid, Vector3 worldHit, bool didHitPlayer, Vector3 localHit, bool isActive, int teamIdHit, Transform justHitTransform)
        {
            validHit = valid;
            worldHitPoint = worldHit;
            hasHitPlayer = didHitPlayer;
            localHitpoint = localHit;
            active = isActive;
            hitTeamId = teamIdHit;
            hitTransform = justHitTransform;
        }
    }



    [Header("Beam Settings")] 
    public float extraRechargeTimeOnDepletion = 4f;
    public float extraRechargeTimeOnCeaseFire = 1f;
    public bool muzzleFlashChildOfBarrel = true;
    public float hitscanRange = 10000f;
    public bool instantBeam = false;

    [Header("Beam Audio")]
    public AudioClip beamStartLoop;
    [Range(0.0f, 1.0f)]
    [SerializeField] protected float beamStartLoopVolume=1f;
    public AudioClip beamEndLoop;
    [Range(0.0f, 1.0f)]
    [SerializeField] protected float beamEndLoopVolume=1f;

    [Header("Prefabs")]
    public GameObject beamLineRendererPrefab;
    public GameObject beamStartPrefab;
    public GameObject beamEndPrefab;

    private int currentBeam = 0;

    private GameObject beamStart;
    private GameObject beamEnd;
    private GameObject beam;
    private LineRenderer line;

    [Header("Adjustable Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned
    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    

    protected bool isRemotelyFiring = false;

    // don't collide with our own colliders
    private Collider[] colliders;

    protected bool isRecharging = false;
    
    // racking override settings
    TurretFollowTarget turretFollowTarget;
    protected GameObject lookpoint;
    
    // internal hit values
    private BeamHit oldHit;
    private BeamHit newHit;
    protected int lastHitId = 0;
    protected bool localFirstFire = false;


    protected void Start()
    {
        Debug.LogWarning("Beam Weapon has not been ported to the new PhysX system");
        return;
        lookpoint = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
    }


    public override void DeselectWeapon()
    {
        CeaseFire();
    }



    public override void CeaseFire()
    {
       // DestroyBeam();
        SetIsRemotelyFiring(false);
        weaponPhotonView.RPC(nameof(DestroyBeam), RpcTarget.All);
        StartCoroutine(DoBonusRecharge(extraRechargeTimeOnCeaseFire));
        
    }

    protected IEnumerator DoBonusRecharge(float amt)
    {
        isRecharging = true;
        yield return new WaitForSeconds(amt);
        isRecharging = false;
    }


    protected new void SetupWeapon()
    {
        colliders = transform.root.GetComponentsInChildren<Collider>();
        newHit = new BeamHit(false, Vector3.zero, false, Vector3.zero, false,0, transform);
        oldHit = new BeamHit(false, Vector3.zero, false, Vector3.zero, false,0, transform);
        lookpoint = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
        base.SetupWeapon();
        colliders = transform.root.GetComponentsInChildren<Collider>();
    }

    public override void ActivateWeapon()
    {
        weaponPhotonView.RPC(nameof(AnimatorSetTriggerNetwork), RpcTarget.All, weaponSelectTriggerName);
        colliders = transform.root.GetComponentsInChildren<Collider>();
        newHit = new BeamHit(false, Vector3.zero, false, Vector3.zero, false,0, transform);
        oldHit = new BeamHit(false, Vector3.zero, false, Vector3.zero, false,0, transform);
        lookpoint = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
        base.ActivateWeapon();
        base.SetupWeapon();
        SetupWeapon();
    }

    public override bool CanFire()
    {
        if (currentSalvo <= 0 && reserveAmmo > 0) {
            ReloadSalvo();
        }
        
        if (isRecharging) return false;
        if((reloadType != ReloadType.noReload) && currentSalvo > 0)return true;
        else if (reloadType == ReloadType.noReload && currentSalvo > 0) return true;
        
        
        
        
        
        /*
        if (currentSalvo <= 0 && reserveAmmo > 0) {
            ReloadSalvo();
        }*/
        
        
        
        return false;
    }


    public override void ResetWeaponToDefaults()
    {
        base.ResetWeaponToDefaults();
        SetIsRemotelyFiring_RPC(false);
    }

    public override void Fire(Vector3 targetPoint)
    {
        
        /*if (gunnerPhotonView.IsMine && !isRemotelyFiring && HasAmmoToShoot())
        {
            
        }*/

        /*
        if (!CanFire() && gunnerPhotonView.IsMine)
        {
            SetIsRemotelyFiring(false);
            CeaseFire();
        }*/





        if (base.CanFire() && gunnerPhotonView.IsMine && CanFire())
        {
            timeSinceLastFire = 0;
            targetPoint =
                CalculateFireDeviation(targetPoint, projectileDeviationDegrees);


            currentCooldown = fireRate;
            UseAmmo(ammoPerShot);
            
            float distanceMultiplier =
                CalculateDamageMultiplierCurve(Vector3.Distance(barrelTransform.position, targetPoint));
            // define weapon damage details
            


            if (!isRemotelyFiring)
            {

                //create beam
                localFirstFire = true;
                CreateBeam();
                SetIsRemotelyFiring(true);
            }
            else
            {
                localFirstFire = false;
            }

            if (!HasAmmoToShoot() && !isRecharging)
            {
                // StartCoroutine(DoBonusRecharge(extraRechargeTimeOnDepletion));

                Debug.Log("cease fire");
                CeaseFire();

            }
            else
            {

                RaycastHitDetails raycastTracerDetails = Fire_HitscanWeaponTracer(targetPoint);






                // if the raycast tracer details health field is not null, then damage em
                if (raycastTracerDetails.hasHealth)
                {
                    WeaponDamageDetails weaponDamageDetails = new WeaponDamageDetails(myNickName, myPlayerId, myTeamId,
                        damageType, baseDamage * distanceMultiplier, raycastTracerDetails.localHitPoint);
                    
                    
                    raycastTracerDetails.hitTransform.gameObject.GetComponentInParent<VehicleHealthManager>()
                        .TakeDamage(weaponDamageDetails);
                }
                // do the fire effect on our end


                // ------------ local firing procedure:
                // if we hit, then fire a ray effect playing hitsound on hit
                if (raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget)
                    FireBeamRoundEffectHit(raycastTracerDetails.worldHitPoint);
                // if we miss, then fire a ray effect playing missound on hit
                else if (!raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget)
                    FireBeamRoundEffectMiss(raycastTracerDetails.worldHitPoint);





                // -----------  remote firing procedure:
                // if optimisations are not enabled, for all other players:

                // if corrections are applied and we hit a target with health then fire a ray using FireHitscanRoundEffectCorrected, playing the hitsound on hit
                if (raycastTracerDetails.hasHealth && raycastTracerDetails.validTarget)
                {
                    int hitTeamId = raycastTracerDetails.hitTransform.GetComponentInParent<NetworkPlayerVehicle>()
                        .teamId;
                    weaponPhotonView.RPC(nameof(FireBeamRoundEffect_RPC), RpcTarget.Others, true,
                        raycastTracerDetails.worldHitPoint, true, raycastTracerDetails.localHitPoint, hitTeamId);
                }
                else if (raycastTracerDetails.validTarget)
                {
                    weaponPhotonView.RPC(nameof(FireBeamRoundEffect_RPC), RpcTarget.Others, true,
                        raycastTracerDetails.worldHitPoint, false, raycastTracerDetails.localHitPoint, 0);
                }
                else
                {
                    weaponPhotonView.RPC(nameof(FireBeamRoundEffect_RPC), RpcTarget.Others, false,
                        raycastTracerDetails.worldHitPoint, false, raycastTracerDetails.localHitPoint, 0);
                }
                

            }
        }

        // now deal with beam effects


        if (beam != null && gunnerPhotonView.IsMine)
        {
            if (localFirstFire && !instantBeam)
            {
                // calculate target point interpolated along targetPoint - barrelEndMuzzleTransform.position direction vector by value timeSinceLastShot / maxLerpTime
                ShotBeamAtPointFirstFireLerp(targetPoint);

            }
            else ShootBeamInDir(barrelEndMuzzleTransform.position, (targetPoint - barrelEndMuzzleTransform.position));
        }
    }

    protected void ShotBeamAtPointFirstFireLerp(Vector3 targetPoint)
    {
        float maxLerpTime = fireRate;
        if (Vector3.Distance(targetPoint, barrelTransform.position) < 10) maxLerpTime = fireRate / 4;
        else if (Vector3.Distance(targetPoint, barrelTransform.position) < 20) maxLerpTime = fireRate / 2;
        float t = timeSinceLastFire / maxLerpTime;
        Vector3 lerpTargetPos = Vector3.Lerp(barrelTransform.position, targetPoint, t);
                
        ShootBeamAtPoint(barrelEndMuzzleTransform.position, lerpTargetPos);
    }

    

    protected void OnDestroy()
    {
        SetIsRemotelyFiring(false);
        DestroyBeam();
    }

    [PunRPC]
    protected void DestroyBeam()
    {
        localFirstFire = true;
        isRemotelyFiring = false;
        Destroy(beamStart);
        Destroy(beamEnd);
        Destroy(beam);
        newHit = new BeamHit(false, Vector3.zero, false, Vector3.zero, false,0, transform);
        oldHit = new BeamHit(false, Vector3.zero, false, Vector3.zero, false,0, transform);
    }


    
    
    

    

    protected void FireBeamRoundEffectMiss(Vector3 targetPoint)
    {
        AnimatorSetTriggerNetwork(primaryFireAnimatorTriggerName);
        // instantiate impact particle with miss sound effect
        //InstantiateImpactEffect(missImpactParticle, targetPoint, impactParticleSoundMiss, missImpactParticleVolume, 2f);
        //PlayImpactSound();
    }

    protected void FireBeamRoundEffectHit(Vector3 targetPoint)
    {
        AnimatorSetTriggerNetwork(primaryFireAnimatorTriggerName);
        // instantiate impact particle with miss sound effect
        InstantiateImpactEffect(imapactParticle, targetPoint, impactParticleSound, imapactParticleVolume, 2f);
        PlayImpactSound();
    }
    
    
    
    // handles the graphics for firing a hitscan round
    // round can leave a mini tracer for now
    [PunRPC]
    protected void FireBeamRoundEffect_RPC(bool validHit, Vector3 worldHit, bool hitPlayer, Vector3 localHit, int hitTeamId)
    {
        lastHitId += 1;
        timeSinceLastFire = 0;
        bool firstFire = false;
        Transform newTargetVehicle = transform;
        
        if (newHit.active) oldHit = newHit;
        
        if(hitPlayer) newTargetVehicle = _playerTransformTracker.GetVehicleTransformFromTeamId(newHit.hitTeamId);
        
        newHit = new BeamHit(validHit, worldHit, hitPlayer, localHit, true, hitTeamId, newTargetVehicle);
        if (oldHit.active == false) oldHit = newHit;
        
        AnimatorSetTriggerNetwork(primaryFireAnimatorTriggerName);
        turretFollowTarget.target = lookpoint;
        
        // if the beam effects are currently null, instantiate them
        if (beam == null || newHit.Equals(oldHit))
        {

            CreateBeam();
            firstFire = true;
        }
        
        // deal effect at last  point to maintain beam / effect consistency
        if (oldHit.hasHitPlayer)
        {
            // get transform from team id
            Transform targetVehicle = _playerTransformTracker.GetVehicleTransformFromTeamId(oldHit.hitTeamId);
            oldHit.hitTransform = targetVehicle;
            // convert local targetpoint into world point
            Vector3 worldPoint = targetVehicle.TransformPoint(oldHit.localHitpoint);
            // instantiate impact at worldPoint
           // InstantiateImpactEffect(imapactParticle, worldPoint, impactParticleSound, imapactParticleVolume, 2f);
            PlayImpactSound();
        }
        
        else if (oldHit.validHit)
        {
           // InstantiateImpactEffect(missImpactParticle, oldHit.worldHitPoint, impactParticleSoundMiss, missImpactParticleVolume, 2f);
        }
        

        // if first fire, then lerp from barrel to target (if option selected)
        if (firstFire && !instantBeam) StartCoroutine(RemoteLerpFirstHit(newHit));
        // otherwise lerp between the two last hitpoints
        else
        {
            StartCoroutine(LerpRemoteHits(oldHit, newHit, lastHitId));
        }
    }

    protected IEnumerator LerpRemoteHits(BeamHit hit1, BeamHit hit2, int currentHitId)
    {
        float elapsedTime = 0f;
        float maxLerpTime = fireRate;

        // save tracking speed
        float oldTrackingSpeed = turretFollowTarget.trackingSpeed;
        float oldDeadZone = turretFollowTarget.deadZone;
        while (elapsedTime < (fireRate))
        {
            float t = elapsedTime / maxLerpTime;
            if(t<1)elapsedTime += Time.deltaTime;
            Vector3 pos1 = Vector3.zero;
            Vector3 pos2 = Vector3.zero;

            
            if (hit1.hasHitPlayer) {
                Debug.Log("hit1: " + hit1);
                Debug.Log("hit1.hitTransform: " + hit1.hitTransform);
                Debug.Log("hit1.localHitpoint: " + hit1.localHitpoint);
                pos1 = hit1.hitTransform.TransformPoint(hit1.localHitpoint);
            }
            else  pos1 = hit1.worldHitPoint;

            if (hit2.hasHitPlayer) {
                Debug.Log("hit2: " + hit2);
                Debug.Log("hit2.hitTransform: " + hit2.hitTransform);
                Debug.Log("hit2.localHitpoint: " + hit2.localHitpoint);
                pos2 = hit2.hitTransform.TransformPoint(hit2.localHitpoint);
            }
            else pos2 = hit2.worldHitPoint;


            Vector3 lerpTargetPos = Vector3.Lerp(pos1, pos2, t);
            ShootBeamAtPoint(barrelEndMuzzleTransform.position, lerpTargetPos);
            // set the lookpoint of the turret script
            lookpoint.transform.position = pos2;
            turretFollowTarget.trackingSpeed = 100;
            turretFollowTarget.deadZone = 0;
            turretFollowTarget.deadZoneTrackingSpeed = 100;
            yield return new WaitForEndOfFrame();
        }

        while (lastHitId == currentHitId)
        {
            Vector3 pos2 = Vector3.zero;
            if (hit2.hasHitPlayer) {
                Debug.Log("hit2: " + hit2);
                Debug.Log("hit2.hitTransform: " + hit2.hitTransform);
                Debug.Log("hit2.localHitpoint: " + hit2.localHitpoint);
                pos2 = hit2.hitTransform.TransformPoint(hit2.localHitpoint);
            }
            else pos2 = hit2.worldHitPoint;
            ShootBeamAtPoint(barrelEndMuzzleTransform.position, pos2);
            lookpoint.transform.position = pos2;

            yield return new WaitForEndOfFrame();
        }
        
        
        // restore tracking speed
        turretFollowTarget.trackingSpeed = oldTrackingSpeed;
        turretFollowTarget.deadZone = oldDeadZone;
    }
    
    // if we instant beam is false, lerp beam to first hit position
    protected IEnumerator RemoteLerpFirstHit(BeamHit firstPos)
    {
        float maxLerpTime = fireRate;
        if (Vector3.Distance(firstPos.worldHitPoint, barrelTransform.position) < 10) maxLerpTime = fireRate / 4;
        else if (Vector3.Distance(firstPos.worldHitPoint, barrelTransform.position) < 20) maxLerpTime = fireRate / 2;

        float elapsedTime = 0f;

        while (elapsedTime < maxLerpTime)
        {
            float t = elapsedTime / maxLerpTime;
            elapsedTime += Time.deltaTime;
            Vector3 targetPoint = Vector3.zero;
            if (firstPos.hasHitPlayer)
            {
                Debug.Log("firstPos: " + firstPos);
                Debug.Log("firstPos.hitTransform: " + firstPos.hitTransform);
                Debug.Log("firstPos.localHitpoint: " + firstPos.localHitpoint);

                targetPoint = firstPos.hitTransform.TransformPoint(firstPos.localHitpoint);
            }
            else
            {
                targetPoint = firstPos.worldHitPoint;
            }
             
            
            Vector3 lerpTargetPos = Vector3.Lerp(barrelTransform.position, targetPoint, t);
            ShootBeamAtPoint(barrelEndMuzzleTransform.position, lerpTargetPos);
            yield return new WaitForEndOfFrame();
        }
    }





    // this is the true shot that we are firing at the enemy
    // fire the ray, determine the world, local
    protected  RaycastHitDetails Fire_HitscanWeaponTracer(Vector3 targetPoint)
    {
        
        // fire a ray from the barrel transform to the targetpoint
        Vector3 startPos = barrelTransform.position;
        
        Ray ray = new Ray(startPos, targetPoint - startPos); 
        RaycastHitDetails raycastHitDetails = FindClosestRaycastHitDetails(ray, targetPoint);
        // we now have our hit details, return themmuzzleFlashChildOfBarrel
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
                if (hit.transform.root.GetComponent<VehicleHealthManager>() != null)
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

    
    protected void CreateBeam()
    {
        
        // create beam should play beam start sound effects
        // we should also lerp to the end hitpoint if that is an enabled option
        InstantiateMuzzleFlash(muzzleflash, 2f, muzzleFlashChildOfBarrel);
        beamStart = Instantiate(beamStartPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        beamEnd = Instantiate(beamEndPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        beam = Instantiate(beamLineRendererPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

        AudioSource bsAudioSource = beamStart.GetComponent<AudioSource>();
        bsAudioSource.clip = beamStartLoop;
        bsAudioSource.volume = beamStartLoopVolume;
        bsAudioSource.loop = true;
        bsAudioSource.Play();
        
        AudioSource bsEndAudioSource = beamEnd.GetComponent<AudioSource>();
        bsEndAudioSource.clip = beamEndLoop;
        bsEndAudioSource.volume = beamEndLoopVolume;
        bsEndAudioSource.loop = true;
        bsEndAudioSource.Play();
        
        line = beam.GetComponent<LineRenderer>();
    }
    
    protected void ShootBeamInDir(Vector3 start, Vector3 dir)
    {
        line.positionCount = 2;

        line.SetPosition(0, start);
        beamStart.transform.position = start;

        Vector3 end = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(start, dir, out hit))
            end = hit.point - (dir.normalized * beamEndOffset);
        else
            end = transform.position + (dir * 100);

        beamEnd.transform.position = end;
        line.SetPosition(1, end);

        beamStart.transform.LookAt(beamEnd.transform.position);
        beamEnd.transform.LookAt(beamStart.transform.position);

        float distance = Vector3.Distance(start, end);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
    }

    protected void ShootBeamAtPoint(Vector3 start, Vector3 end)
    {
        if (beam != null)
        {
            line.positionCount = 2;

            line.SetPosition(0, start);
            beamStart.transform.position = start;



            beamEnd.transform.position = end;
            line.SetPosition(1, end);

            beamStart.transform.LookAt(beamEnd.transform.position);
            beamEnd.transform.LookAt(beamStart.transform.position);

            float distance = Vector3.Distance(start, end);
            line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
            line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
        }
    }
    
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

        isRemotelyFiring = set;
    }
    [PunRPC]
    protected void SetIsRemotelyFiring_RPC(bool set)
    {
        isRemotelyFiring = set;
        PhotonTurretView turretView = transform.root.GetComponentInChildren<PhotonTurretView>();
        turretFollowTarget = transform.root.GetComponentInChildren<TurretFollowTarget>();
        if (isRemotelyFiring == false)
        {
            DestroyBeam();
            if (!weaponPhotonView.IsMine)
            {
                turretFollowTarget.enabled = false;
                turretView.enabled = true;
            }
        }
        else if (isRemotelyFiring && !weaponPhotonView.IsMine)
        {
            turretView.enabled = false;
            turretFollowTarget.enabled = true;
        }
    }

    protected IEnumerator InterpolateBeamState()
    {
        while (isRemotelyFiring)
        {
            yield return new WaitForEndOfFrame();
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

    void PlayImpactSound()
    {
        beamEnd.GetComponent<AudioSource>().PlayOneShot(impactParticleSound, imapactParticleVolume);
    }

    protected void InstantiateMuzzleFlash(GameObject effect, float lifetime, bool childOfMuzzle)
    {
        GameObject mFlash;
        mFlash = Instantiate(effect, barrelTransform.position, barrelTransform.rotation);
        if(childOfMuzzle)mFlash.transform.SetParent(barrelTransform);
        PlayAudioClipOneShot(weaponFireSound);
        Destroy(mFlash, lifetime);
    }
}