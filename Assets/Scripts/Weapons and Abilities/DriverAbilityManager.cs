using System;
using System.Collections;
using System.Collections.Generic;
using Gamestate;
using Photon.Pun;
using UnityEngine;
using PhysX;



[VehicleScript(ScriptType.playerDriverScript)]
// [VehicleScript(ScriptType.playerDriverScript)]

public class DriverAbilityManager : MonoBehaviour, IPunObservable, ICollisionEnterEvent
{


    public DriverAbility abilityPrimary;
    public DriverAbilitySmall abilitySecondary;

    public float driverUltimateProgress;

    public float maxDriverUltimateProgress = 100f;

    public bool usingUltimate = false;

    private PhotonView driverPhotonView;

    private bool isDriver = false;

    private UltimateUiManager ultimateUiManager;


    private GamestateTracker gamestateTracker;

    private int driverId;
    private int gunnerId;
    private bool driverBot;
    private bool gunnerBot;

    public bool pauseableAbility = false;

    private GunnerWeaponManager gunnerWeaponManager;

    float lastGunnerDamageDealt = 0;
    float gunnerDamageDealt = 0;
    public float chargeIncreasePerDamagePoint = 1;


    private bool isSetup = false;

    private bool isHost = false;
    // driver keeps track of this
    
    
    public void CollisionEnter() {}

    public void CollisionEnter(PhysXCollision collision) {
            if (collision.gameObject.GetComponent<VehicleHealthManager>() != null)
            {
                abilityPrimary.JustCollided();
            }
        else if(collision.gameObject.layer == 6)abilityPrimary.JustCollided();
    }

    public bool requiresData { get { return true; } }
    
    public void SetupDriverAbilityManager()
    {
        isHost = PhotonNetwork.IsMasterClient;
        ultimateUiManager = FindObjectOfType<UltimateUiManager>();
        gamestateTracker = FindObjectOfType<GamestateTracker>();
        driverPhotonView = GetComponent<PhotonView>();
        NetworkPlayerVehicle npv = GetComponent<NetworkPlayerVehicle>();
        if (npv != null)
        {
            driverId = npv.GetDriverID();
            if (driverId == PhotonNetwork.LocalPlayer.ActorNumber) isDriver = true;
            gunnerId = npv.GetGunnerID();
            if (driverId < 0) driverBot = true;
            if (gunnerId < 0) gunnerBot = true;
            AdjustDriverUltimateProgress(0);
        }
        abilityPrimary.SetupAbility();
        abilitySecondary.SetupAbility();

        gunnerWeaponManager = GetComponentInChildren<GunnerWeaponManager>();
        
        isSetup = true;
        //  abilitySecondary.SetupAbility();
    }


    
    
    
    
    public void AdjustDriverUltimateProgress(float amt)
    {
        driverUltimateProgress += amt;
        if (driverUltimateProgress < 0) driverUltimateProgress = 0;
        if (driverUltimateProgress > maxDriverUltimateProgress) driverUltimateProgress = maxDriverUltimateProgress;
    }

    public void SetDriverUltimateProgress(float amt)
    {
        driverUltimateProgress = amt;
        if (driverUltimateProgress < 0) driverUltimateProgress = 0;
        if (driverUltimateProgress > maxDriverUltimateProgress) driverUltimateProgress = maxDriverUltimateProgress;
        
        driverPhotonView.RPC(nameof(SetDriverUltimateProgress_RPC), RpcTarget.All, driverUltimateProgress);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(driverUltimateProgress);
        }
        else
        {
            driverUltimateProgress = (float)stream.ReceiveNext();
        }
    }
   

    [PunRPC]
    void SetDriverUltimateProgress_RPC(float currentCharge)
    {
        driverUltimateProgress = currentCharge;
        if (driverUltimateProgress == 0) usingUltimate = false;
        
        // now update the current hud with ultimate progress, if we are a real player
        if (driverId == PhotonNetwork.LocalPlayer.ActorNumber || gunnerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            if(ultimateUiManager!=null) ultimateUiManager.UpdateDriverBar(driverUltimateProgress, maxDriverUltimateProgress);
        }
    }

    public void Reset()
    {
        driverPhotonView.RPC(nameof(ResetDriverAbilityManager_RPC), RpcTarget.All);
    }

    private void OnCollisionEnter(Collision other)
    {

    }

    [PunRPC]
    void ResetDriverAbilityManager_RPC()
    {
        DeactivatePrimaryAbility();
       // SetDriverUltimateProgress(0);
        abilityPrimary.ResetAbility();
    }


    void Update()
    {
        if (isDriver && Input.GetButtonDown("Ultimate") && !usingUltimate &&
            driverUltimateProgress >= maxDriverUltimateProgress)
        {
            SetUsingDriverUltimate(true);
            abilityPrimary.SetLockOn(false);
        }


        if(driverUltimateProgress >= maxDriverUltimateProgress && isDriver){
            abilitySecondary.SetLockOn(false);
        }

        else if(abilitySecondary.CanUseAbility() && isDriver){
            abilitySecondary.SetLockOn(true);
        }
         else if(!abilitySecondary.CanUseAbility() &&isDriver){
            abilitySecondary.SetLockOn(false);
        }
        

        // if the ability is sustained, just burn it out
        if (isDriver && usingUltimate && !pauseableAbility)
        {
            FirePrimaryAbility();
            abilityPrimary.SetLockOn(false);
        }

        if(isDriver && Input.GetKeyDown(KeyCode.E) && abilitySecondary.CanUseAbility()){
            if(abilitySecondary!=null) abilitySecondary.Fire();
        }
        
        // if we can pause the ability (ie hold down space)
        else if (isDriver && Input.GetButtonDown("Ultimate") && usingUltimate && pauseableAbility)
        {
            FirePrimaryAbility();
            abilityPrimary.SetLockOn(false);
        }

        if (isDriver && Input.GetButtonUp("Ultimate") && usingUltimate && pauseableAbility)
        {
            CeasePrimaryAbility();
            abilityPrimary.SetLockOn(false);
        }

        if (!usingUltimate &&
            driverUltimateProgress >= maxDriverUltimateProgress && isDriver && abilityPrimary.isLockOnAbility)
        {
            abilityPrimary.SetLockOn(true);
        }

        
        if(isSetup && (isDriver  || (driverBot  && isHost))){
           // Debug.Log("updating gunner damage dealt in ability manager");
            gunnerDamageDealt = gunnerWeaponManager.gunnerDamageDealt;
            if (lastGunnerDamageDealt < gunnerDamageDealt)
            {
                float difference = gunnerDamageDealt - lastGunnerDamageDealt;

                lastGunnerDamageDealt = gunnerDamageDealt;

                if (!usingUltimate)
                {
                    float increaseCharge = difference * chargeIncreasePerDamagePoint;
                    AdjustDriverUltimateProgress(increaseCharge);
                }
            }
        }

        if (driverId == PhotonNetwork.LocalPlayer.ActorNumber || gunnerId == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            // update hud
            ultimateUiManager.UpdateDriverBar(driverUltimateProgress, maxDriverUltimateProgress);

        }




    }


    void FirePrimaryAbility()
    {
        if (abilityPrimary != null)
        {
            abilityPrimary.Fire();
        }
    }
    void CeasePrimaryAbility()
    {
        if (abilityPrimary != null)
        {
            abilityPrimary.CeaseFire();
        }
    }

    public void DeactivatePrimaryAbility()
    {
        if (abilityPrimary != null)
        {
            abilityPrimary.CeaseFire();
            abilityPrimary.DeactivateAbility();
        }
    }
    
    
    [PunRPC]
    void SetUsingDriverUltimate_RPC(bool set)
    {
        if (isDriver)
        {
            if (set)
            {
                abilityPrimary.ActivateAbility();
                //  abilitySecondary.ActivateAbility();
            }
            else
            {
                abilityPrimary.DeactivateAbility();
                //  abilitySecondary.DeactivateAbility();
            }
        }

        usingUltimate = set;
    }


    public void SetUsingDriverUltimate(bool set)
    {
        if(set!=usingUltimate && isDriver) driverPhotonView.RPC(nameof(SetUsingDriverUltimate_RPC), RpcTarget.All, set);
        usingUltimate = set;
    }
}
