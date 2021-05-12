using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Weapon : Equipment
{
    public enum ReloadType
    {
        recharge,
        byClip,
        noReload
    }
    
    public enum DamageType
    {
        kinetic,
        energy,
        thermal,
        ramming,
        explosive
    }

    
    [Serializable]
    public struct WeaponDamageDetails
    {
        public string sourcePlayerNickName;
        public int sourcePlayerId;
        public int sourceTeamId;
        public DamageType damageType;
        public float damage;
        public Vector3 localHitPoint;
        
        public WeaponDamageDetails(string nickName, int id, int teamId ,DamageType dt, float d, Vector3 localHp)
        {
            sourcePlayerId = id;
            sourceTeamId = teamId;
            sourcePlayerNickName = nickName;
            damageType = dt;
            damage = d;
            localHitPoint = localHp;
        }
    }

    

    protected PlayerTransformTracker _playerTransformTracker;

    protected float timeSinceLastFire = 0f;

    protected string myNickName = "null";

    protected int myPlayerId = 0;

    protected VehicleHealthManager myVehicleManager;

    protected int myTeamId = 0;

    protected GunnerWeaponManager gunnerWeaponManager;
    
    public PhysXCollider.CollisionLayer raycastLayers;


    // Start is called before the first frame update
    [Header("Primary Properties")]
    public string weaponName = "defaultWeapon";
    public bool isUltimate = false;
    public bool returnToFirstWeaponGroupOnEmpty = true;
    [Header("Self Transform")]
    public Transform barrelTransform;
    public Transform barrelEndMuzzleTransform;
    [Header("Damage Falloff and aiming")]
    public float projectileDeviationDegrees = 0f;
    public AnimationCurve damageRampupMultiplierCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public float damageMultiplierPlateuDistance = 100f;
    public float damageMultiplierClosestRampupThreshold = 10f;
    [Header("Salvo and Reloading")]
    [SerializeField]
    protected int salvoSize = 1;
    protected int currentSalvo=0;
    [SerializeField] protected ReloadType reloadType;
    [SerializeField] protected float reloadTime = 1f;
    [SerializeField] public float fireRate = 0.5f;
    protected float reloadProgress = 0f;
    protected float currentCooldown = 0f;
    [Header("Ammunition")]
    public int ammoPerShot = 1;
    public bool unlimitedAmmo = false;
    public int reserveAmmo = 100;
    public int maxReserveAmmo = 100;
    public bool fullSalvoOnStart = true;

    protected int defaultSalvoSize;
    protected ReloadType defaultReloadType;
    protected float defaultReloadTime;
    protected float defaultFireRate;
    protected int defaultAmmoPerShot;
    protected bool defaultUnlimitedAmmo;
    protected int defaultReserveAmmo;


    [Header("Damage")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected DamageType damageType;
    [SerializeField] protected float damageMultiplier = 1f;
    
    
    [Header("Animation")]
    
    [SerializeField] protected Animator weaponAnimator;
    [SerializeField] protected string weaponSelectTriggerName = "Primary";
    [SerializeField] protected string reloadAnimatorTriggerName = "Reload";
    [SerializeField] protected string primaryFireAnimatorTriggerName = "Fire";
    [Header("Network")]
    public PhotonView weaponPhotonView;
    public PhotonView gunnerPhotonView;

    private NetworkPlayerVehicle _networkPlayerVehicle;
  //  public Transform sourceCam;
    
   // [Header("UI")]
   
   // weaponUI is found dynamically in scene
   // it is a prefab
    protected WeaponUi weaponUi;

    private bool isSetup = false;

     [Header("Audio")]
    public AudioClip weaponFireSound;
    [Range(0.0f, 1.0f)]
    [SerializeField] protected float muzzleflashVolume=1f;
    public AudioClip impactParticleSound;
    [Range(0.0f, 1.0f)]
    [SerializeField] protected float imapactParticleVolume=1f;
    public AudioClip impactParticleSoundMiss;
    [Range(0.0f, 1.0f)]
    [SerializeField] protected float missImpactParticleVolume=0.75f;
    public GameObject audioSourcePrefab;
    
    [Header("Effects")]
    [SerializeField] protected GameObject muzzleflash;
    
    [SerializeField] protected GameObject imapactParticle;
    

    [SerializeField] protected GameObject missImpactParticle;

    [SerializeField] protected float cameraShakeAmplitude = 2f;
    [SerializeField] protected float cameraShakeDuration = 0.1f;

    protected CinemachineVirtualCamera turretCam;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    protected DriverCinematicCam driverCamScript;
    protected float shakeTimerMax = 0;
    protected float shakeTimerCur= 0;

    protected uint rigidbodyVehicleId;
    protected void Awake()
    {
        defaultSalvoSize = salvoSize;
        defaultReloadType = reloadType;
        defaultReloadTime = reloadTime;
        defaultFireRate = fireRate;
        defaultAmmoPerShot = ammoPerShot;
        defaultUnlimitedAmmo = unlimitedAmmo;
        defaultReserveAmmo = reserveAmmo;

        turretCam = transform.root.GetComponentInChildren<PlayerGunnerController>().camera;
        cinemachineBasicMultiChannelPerlin = turretCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        driverCamScript = transform.root.GetComponentInChildren<DriverCinematicCam>();
        rigidbodyVehicleId = GetComponentInParent<PhysXRigidBody>().vehicleId;
    }





    protected void ShakeCameras(float intensity, float time)
    {

        shakeTimerMax = time;
        shakeTimerCur = 0;
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
        driverCamScript.ShakeCams(intensity, time);
    }

    public virtual void ResetWeaponToDefaults()
    {
        salvoSize = defaultSalvoSize;
        reloadType = defaultReloadType;
        reloadTime = defaultReloadTime;
        fireRate = defaultFireRate;
        ammoPerShot = defaultAmmoPerShot;
        unlimitedAmmo = defaultUnlimitedAmmo;
        reserveAmmo = defaultReserveAmmo;
        
    }

    protected void PlayAudioClipOneShot(AudioClip clip)
    {
        if (audioSourcePrefab != null)
        {
            GameObject audioInstance = Instantiate(audioSourcePrefab, barrelTransform.position, barrelTransform.rotation);
            audioInstance.GetComponent<AudioSource>().PlayOneShot(weaponFireSound, muzzleflashVolume);
            Destroy(audioInstance, weaponFireSound.length);
        }
    }
    public virtual void Fire(Vector3 targetPoint){

    }

    public virtual void CeaseFire()
    {
    }

    public virtual void DeselectWeapon()
    {
        
    }

    public virtual void PickupAmmo(int amt)
    {
        if (reloadType == ReloadType.noReload)
        {
            currentSalvo += amt;
            if (currentSalvo > salvoSize) currentSalvo = salvoSize;
        }
        else
        {
            reserveAmmo += amt;
            if (reserveAmmo > maxReserveAmmo) reserveAmmo = maxReserveAmmo;
        }
        
        UpdateHud();
    }

    protected Vector3 CalculateFireDeviation(Vector3 oldTargetPoint, float maxDegrees)
    {
        if (maxDegrees == 0) return oldTargetPoint;
        float deviationDegreesTraverse = Random.Range(0, maxDegrees);
        float deviationDegreesElevation = Random.Range(0, maxDegrees);
        // get vector distance from barrel to hitpoint
        float range = Vector3.Distance(oldTargetPoint, barrelTransform.position);

        float max = Mathf.Tan(Mathf.Deg2Rad * maxDegrees) * range;


        Vector3 deviation3D = Random.insideUnitSphere * max;



        Vector3 newTargetPoint = oldTargetPoint + deviation3D;
        
        
        return newTargetPoint;
    }

    // only called on success
    // deals with firing the actual projectiles, and lag compensated dummy ones
    [PunRPC]
    protected void FireRPC(Vector3 targetPoint, float distanceDamageMultiplier)
    {
        
        // debug function to fire weapon
        GameObject go = new GameObject();
        go.AddComponent<AudioSource>();
        GameObject goInstance = Instantiate(go, barrelTransform.position, barrelTransform.rotation);
        if(weaponFireSound!=null) goInstance.GetComponent<AudioSource>().PlayOneShot(weaponFireSound);
        Debug.Log("Default weapon class object has fired, why the hell are you using this script you plonker");
        Destroy(goInstance, 3f);
    }
    
    //-----------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------
    
    public virtual void SetupWeapon()
    {   
        // assign photon view to the gunner
        //Player gunnerPlayer = gunnerPhotonView.Owner;
        
        _networkPlayerVehicle = GetComponentInParent<NetworkPlayerVehicle>();
        myVehicleManager = GetComponentInParent<VehicleHealthManager>();
        gunnerWeaponManager = gunnerPhotonView.GetComponent<GunnerWeaponManager>();
        if (_networkPlayerVehicle != null)
        {
            myNickName = _networkPlayerVehicle.GetGunnerNickName();
            myPlayerId = _networkPlayerVehicle.GetGunnerID();
            myTeamId = _networkPlayerVehicle.teamId;
        }
        else
        {
            Debug.LogError("Weapon does not belong to a valid vehicle!! Assigning owner to null");
        }

        //weaponPhotonView.TransferOwnership(gunnerPlayer);

        weaponUi = FindObjectOfType<WeaponUi>();
        _playerTransformTracker = FindObjectOfType<PlayerTransformTracker>();
        if (fullSalvoOnStart) currentSalvo = salvoSize;
        isSetup = true;
    }

    // called to activate UI elements and transfer photonview
    public virtual void ActivateWeapon()
    {
        if (!isSetup) SetupWeapon();
        
        if(gunnerPhotonView!=null && weaponUi!=null){ if (gunnerPhotonView.IsMine && !_networkPlayerVehicle.botGunner) weaponUi.SetCanvasVisibility(true);}
        
        weaponPhotonView.RPC(nameof(AnimatorSetTriggerNetwork), RpcTarget.All, weaponSelectTriggerName);

        UpdateHud();
    }

    public virtual void ActivateWeaponInternal()
    {
    }

    // temporary fire solutionUpdateHud


    protected void UpdateHud()
    {
        if (weaponUi != null && gunnerPhotonView.IsMine && myPlayerId == gunnerPhotonView.Controller.ActorNumber)
        {
            weaponUi.UpdateAmmo(currentSalvo, salvoSize, reserveAmmo);
            weaponUi.SetWeaponNameText(weaponName);
        }
        
    }

    protected void UseAmmo(int amt)
    {

            DecrementSalvo(amt);
            UpdateHud();
            GunnerUltimateUpdateCallback();
    }

    protected void ReduceReserveAmmo(int amt)
    {
        if(!unlimitedAmmo)reserveAmmo -= amt;
        if (reserveAmmo < 0) reserveAmmo = 0;
        UpdateHud();
    }
    protected void DecrementSalvo(int amt){
        currentSalvo -=amt;
        if(currentSalvo < 0) currentSalvo = 0;
        UpdateHud();
    }
    protected void ReloadShells(int amount)
    {
        if (amount == 0) return;
        if (currentSalvo >= salvoSize) return;
        if (reloadType == ReloadType.noReload) return;
        if (amount > reserveAmmo)
        {
            amount = reserveAmmo;
        }

        if ((amount-currentSalvo) > salvoSize)
        {
            amount = amount - currentSalvo;
        }
        currentSalvo += amount;
        if(!unlimitedAmmo) reserveAmmo -= amount;
        
        if(currentSalvo > salvoSize) currentSalvo = salvoSize;
        GunnerUltimateUpdateCallback();
        UpdateHud();
        
    }
    protected void ReloadFull()
    {
        if (reserveAmmo <= 0) return;
        int diff = salvoSize - currentSalvo;

        if (diff > reserveAmmo && reloadType != ReloadType.noReload)
        {
            currentSalvo += reserveAmmo;
        }
        else
        {
            currentSalvo += salvoSize;
        }
        
       
        if(currentSalvo > salvoSize) currentSalvo = salvoSize;

        if (diff > reserveAmmo) reserveAmmo = diff;
        
        if (reloadType == ReloadType.byClip)
        {
            ReduceReserveAmmo(diff);
        }
        UpdateHud();
    }

    protected void Update()
    {
        timeSinceLastFire += Time.deltaTime;
        
        shakeTimerCur += Time.deltaTime;
        if (isSetup && shakeTimerCur <= shakeTimerMax)
        {
            if (gunnerWeaponManager.currentWeaponControlGroup.weapons.Contains(this))
            {
                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain =
                    Mathf.Lerp(cameraShakeAmplitude, 0f, (shakeTimerCur / shakeTimerMax));
            }
        }



        if (currentCooldown >= 0)
        {
            currentCooldown -= Time.deltaTime;
        }
        
        
        if (reloadType == ReloadType.recharge && timeSinceLastFire > reloadTime)
        {
            // recharge a single round in the salvo
            if (currentSalvo < salvoSize)
            {
                reloadProgress += Time.deltaTime;
                if (reloadProgress > reloadTime)
                {
                    ReloadShells(1);
                    reloadProgress = 0f;
                }
            }
        }
        
        
    }

    [PunRPC]
    protected void AnimatorSetTriggerNetwork(string triggerName)
    {
        if (weaponAnimator != null)
        {
         weaponAnimator.SetTrigger(triggerName);
        }
    }
    

    // called manually by player / ai to reload the gun
    public void ReloadSalvo()
    {
        if (reloadType == ReloadType.byClip && reserveAmmo > 0)
        {

            currentCooldown = reloadTime;

            ReloadBehaviour reloadIcon = FindObjectOfType<ReloadBehaviour>();
            if (reloadIcon != null) StartCoroutine(reloadIcon.Reload(reloadTime));

            weaponPhotonView.RPC(nameof(AnimatorSetTriggerNetwork), RpcTarget.All, reloadAnimatorTriggerName);
            
            ReloadFull();
        }
        GunnerUltimateUpdateCallback();
    }

    protected void GunnerUltimateUpdateCallback()
    {
        if (isUltimate)
        {
            float amt = 0;
            // find out how much of a fraction of starting ammo a single shot is
            if (reloadType == ReloadType.noReload)
            {
                amt = (float) currentSalvo / (float) salvoSize;
            }
            else
            {
                if (fullSalvoOnStart) amt = (currentSalvo + reserveAmmo) / (salvoSize + defaultReserveAmmo);

                else amt = (currentSalvo + reserveAmmo) / (defaultReserveAmmo);


            }

            amt *= 100;
         //   Debug.Log("amt" + amt);
            gunnerWeaponManager.SetGunnerUltimateProgress(amt * (gunnerWeaponManager.maxGunnerUltimateProgress / 100));
        }
    }

    protected void SelectFirstIfEmpty()
    {
        if (returnToFirstWeaponGroupOnEmpty)
        {
            if (reloadType == ReloadType.noReload && currentSalvo < ammoPerShot)
            {
                Debug.Log("out of ammo" + this);
                gunnerPhotonView.gameObject.GetComponent<GunnerWeaponManager>().SelectFirst();
                //return false;
            }
            else if ((reloadType != ReloadType.noReload) && currentSalvo < ammoPerShot && reserveAmmo < ammoPerShot)
            {
                Debug.Log("out of ammo" + this);
                gunnerPhotonView.gameObject.GetComponent<GunnerWeaponManager>().SelectFirst();
                //   return false;
            }
        }
    }

    public virtual bool CanFire()
    {
        
        if (currentSalvo <= 0 && reserveAmmo > 0) {
            ReloadSalvo();
        }
        
        
        if (currentCooldown <= 0 && myVehicleManager.health > 0)
        {
            GunnerUltimateUpdateCallback();
            if((reloadType != ReloadType.noReload) && currentSalvo > 0)return true;
            else if (reloadType == ReloadType.noReload && currentSalvo > 0) return true;
        }

        SelectFirstIfEmpty();

        
        
        

        return false;
    }

    protected bool HasAmmoToShoot()
    {
        if((reloadType != ReloadType.noReload) && currentSalvo > 0)return true;
        else if (reloadType == ReloadType.noReload && currentSalvo > 0) return true;
        
        SelectFirstIfEmpty();
        
        
        return false;
    }
    protected bool PenultimateShot()
    {
        if((reloadType != ReloadType.noReload) && currentSalvo >= ammoPerShot)return false;
        else if (reloadType == ReloadType.noReload && reserveAmmo >= ammoPerShot) return false;
        else return true;
    }

    protected float CalculateDamageMultiplierCurve(float distance)
    {
        float damageRampupMultiplier = 1f;
        if(distance < damageMultiplierPlateuDistance){
            // calculate value
            if(distance < damageMultiplierClosestRampupThreshold){
                damageRampupMultiplier = damageRampupMultiplierCurve.Evaluate(0f);
            }
            else{
                float fraction = (distance - damageMultiplierClosestRampupThreshold)/(damageMultiplierPlateuDistance-damageMultiplierClosestRampupThreshold);
                damageRampupMultiplier = damageRampupMultiplierCurve.Evaluate(fraction);
            }
           
        }
        return damageRampupMultiplier;
    }
    
    // ----------------------------------- TEMP

}
