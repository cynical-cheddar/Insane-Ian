using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
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
        public WeaponDamageDetails(string nickName, int id, int teamId ,DamageType dt, float d)
        {
            sourcePlayerId = id;
            sourceTeamId = teamId;
            sourcePlayerNickName = nickName;
            damageType = dt;
            damage = d;
        }
    }

    protected PlayerTransformTracker _playerTransformTracker;

    protected float timeSinceLastFire = 0f;

    protected string myNickName = "null";

    protected int myPlayerId = 0;

    protected int myTeamId = 0;
    // Start is called before the first frame update
    [Header("Primary Properties")]
    public bool fullClipOnSpawn = true;
    public string weaponName = "defaultWeapon"; 
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
    [SerializeField] protected float fireRate = 0.5f;
    protected float reloadProgress = 0f;
    protected float currentCooldown = 0f;
    [Header("Ammunition")]
    public int ammoPerShot = 1;
    public bool unlimitedAmmo = false;
    public int reserveAmmo = 100;
    [Header("Damage")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected DamageType damageType;
    [SerializeField] protected float damageMultiplier = 1f;
    
    
    [Header("Animation")]
    [SerializeField] protected Animator weaponAnimator;
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
    
    // ---------------------- COPY THESE FUNCTIONS FOR EACH CHILD CLASS --------------------------------//
    // ---------------------- RPCs DO NOT INHERIT FROM PARENT ------------------------------------------//
    
    // method called by weaponController
    // we need a new version of this for every child class, otherwise the top level RPC will be called

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

    protected void Start()
    {
        if (fullClipOnSpawn)
        {
            currentSalvo = salvoSize;
        }
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
        ReloadSalvo();
        isSetup = true;
    }

    // called to activate UI elements and transfer photonview
    public virtual void ActivateWeapon()
    {
        if (!isSetup) SetupWeapon();
        
        if(gunnerPhotonView!=null){ if (gunnerPhotonView.IsMine && !_networkPlayerVehicle.botGunner) weaponUi.SetCanvasVisibility(true);}
        

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
        // use clips and salvos
        if (reloadType != ReloadType.noReload)
        {
            DecrementSalvo(amt);
        }
        // use reserve ammo
        else
        {
            ReduceReserveAmmo(amt);
        }
        UpdateHud();
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
        UpdateHud();
    }
    protected void ReloadFull(){
        currentSalvo += salvoSize;
        if(currentSalvo > salvoSize) currentSalvo = salvoSize;
        UpdateHud();
    }

    protected void Update()
    {
        timeSinceLastFire += Time.deltaTime;
        
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
        if (reloadType == ReloadType.byClip)
        {
            currentCooldown = reloadTime;
            
            weaponPhotonView.RPC(nameof(AnimatorSetTriggerNetwork), RpcTarget.All, reloadAnimatorTriggerName);
            
            ReloadFull();
        }
    }
    


    public virtual bool CanFire()
    {
        if (currentCooldown <= 0)
        {
            if((reloadType != ReloadType.noReload) && currentSalvo > 0)return true;
            else if (reloadType == ReloadType.noReload && reserveAmmo >= ammoPerShot) return true;
        }

        return false;
    }

    protected bool HasAmmoToShoot()
    {
        if((reloadType != ReloadType.noReload) && currentSalvo > 0)return true;
        else if (reloadType == ReloadType.noReload && reserveAmmo >= ammoPerShot) return true;
        else return false;
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
