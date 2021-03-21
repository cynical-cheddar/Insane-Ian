using System.Collections;
using System.Collections.Generic;
using Gamestate;
using Photon.Pun;
using UnityEngine;






public class DriverAbilityManager : MonoBehaviour
{

    public DriverAbility abilityPrimary;

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
    
    
    void Start()
    {
        isHost = PhotonNetwork.IsMasterClient;
        Invoke(nameof(SetupDriverAbilityManager), 0.5f);
    }
    
    public void SetupDriverAbilityManager()
    {
        
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

        gunnerWeaponManager = GetComponentInChildren<GunnerWeaponManager>();
        
        isSetup = true;
        //  abilitySecondary.SetupAbility();
    }


    
    
    
    
    public void AdjustDriverUltimateProgress(float amt)
    {
        driverUltimateProgress += amt;
        if (driverUltimateProgress < 0) driverUltimateProgress = 0;
        if (driverUltimateProgress > maxDriverUltimateProgress) driverUltimateProgress = maxDriverUltimateProgress;
        
        driverPhotonView.RPC(nameof(SetDriverUltimateProgress_RPC), RpcTarget.All, driverUltimateProgress);
    }

    public void SetDriverUltimateProgress(float amt)
    {
        driverUltimateProgress = amt;
        if (driverUltimateProgress < 0) driverUltimateProgress = 0;
        if (driverUltimateProgress > maxDriverUltimateProgress) driverUltimateProgress = maxDriverUltimateProgress;
        
        driverPhotonView.RPC(nameof(SetDriverUltimateProgress_RPC), RpcTarget.All, driverUltimateProgress);
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

    [PunRPC]
    void ResetDriverAbilityManager_RPC()
    {
        DeactivatePrimaryAbility();
        SetDriverUltimateProgress(0);
        abilityPrimary.ResetAbility();
    }


    void Update()
    {
        if (isDriver && Input.GetButtonDown("Jump") && !usingUltimate &&
            driverUltimateProgress >= maxDriverUltimateProgress)
        {
            SetUsingDriverUltimate(true);
        }

        // if the ability is sustained, just burn it out
        if (isDriver && usingUltimate && !pauseableAbility)
        {
            FirePrimaryAbility();
        }
        
        // if we can pause the ability (ie hold down space)
        else if (isDriver && Input.GetButton("Jump") && usingUltimate && pauseableAbility)
        {
            FirePrimaryAbility();
        }

        if (isDriver && Input.GetButtonUp("Jump") && usingUltimate && pauseableAbility)
        {
            CeasePrimaryAbility();
        }

        
        if(isSetup && (isDriver  || (driverBot  && isHost))){
        
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
        usingUltimate = set;
    }


    public void SetUsingDriverUltimate(bool set)
    {
        if(set!=usingUltimate) driverPhotonView.RPC(nameof(SetUsingDriverUltimate_RPC), RpcTarget.All, set);
        usingUltimate = set;
    }
}
