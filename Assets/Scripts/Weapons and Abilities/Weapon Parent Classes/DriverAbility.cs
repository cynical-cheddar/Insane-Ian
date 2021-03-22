using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DriverAbility : Equipment
{
    protected float currentCooldown = 0f;
    public float cooldown = 1f;
    protected float timeSinceLastFire = 0f;

    protected PhotonView driverPhotonView;
    protected PhotonView abilityPhotonView;
    protected NetworkPlayerVehicle _networkPlayerVehicle;
    protected VehicleManager myVehicleManager;
    protected DriverAbilityManager driverAbilityManager;
        
    protected int myPlayerId;
    protected int myTeamId;
    protected string myNickName = "";

    
    protected bool isSetup = false;

    protected bool abilityActivated = false;

    public AudioClip activateAudioClip;

    public int maxCharge = 100;
    public int currentCharge = 0;
    public int chargeUsedPerFire = 5;
    
    protected virtual void Update()
    {
        timeSinceLastFire += Time.deltaTime;

        if (currentCooldown >= 0)
        {
            currentCooldown -= Time.deltaTime;
        }

    }


    protected bool CanUseAbility()
    {
        if (currentCooldown <= 0 && driverAbilityManager.usingUltimate && myVehicleManager.health > 0 && abilityActivated)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected void UseCharge(int amt)
    {
        currentCharge -= amt;
        if (currentCharge < 0) currentCharge = 0;
        
        // update hud
        
        
        driverAbilityManager.SetDriverUltimateProgress(currentCharge);
        
        if (currentCharge == 0)
        {
            driverAbilityManager.DeactivatePrimaryAbility();
        }
        
    }
   

    public virtual void SetupAbility()
    {
        driverPhotonView = transform.root.GetComponent<PhotonView>();
        // assign photon view to the driver
        abilityPhotonView = GetComponent<PhotonView>();
        abilityPhotonView.TransferOwnership(driverPhotonView.Owner);
        
        
        //Player gunnerPlayer = gunnerPhotonView.Owner;

        _networkPlayerVehicle = driverPhotonView.GetComponent<NetworkPlayerVehicle>();
        myVehicleManager = driverPhotonView.GetComponent<VehicleManager>();
        driverAbilityManager = driverPhotonView.GetComponent<DriverAbilityManager>();

        if (_networkPlayerVehicle != null)
        {
            myNickName = _networkPlayerVehicle.GetDriverNickName();
            myPlayerId = _networkPlayerVehicle.GetDriverID();
            myTeamId = _networkPlayerVehicle.teamId;
        }
        else
        {
            Debug.LogError("Ability does not belong to a valid vehicle!! Assigning owner to null");
        }

        isSetup = true;
    }

    public virtual void ResetAbility()
    {
        
    }

    public virtual void ActivateAbility()
    {
        
        if (!isSetup)
        {
            SetupAbility();
        }

        currentCharge = maxCharge;
        abilityActivated = true;
       // Debug.Log("Activate driver ability");
        abilityPhotonView.RPC(nameof(ActivationEffects_RPC), RpcTarget.All);
    }

    public virtual void DeactivateAbility()
    {
        abilityActivated = false;
        Debug.Log("Deactivate driver ability");
    }

    [PunRPC]
    protected virtual void ActivationEffects_RPC()
    {
        GetComponent<AudioSource>().PlayOneShot(activateAudioClip);
    }



    public override void Fire()
    {
        if (CanUseAbility())
        {
            currentCooldown = cooldown;
            timeSinceLastFire = 0;
            Debug.Log("Base driver ability fire");
        }
    }

    public override void CeaseFire()
    {
        base.CeaseFire();
    }
}
