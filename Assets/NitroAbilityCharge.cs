using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class NitroAbilityCharge : DriverAbility
{
    // Start is called before the first frame update
    public float activationVelocityChange = 5f;



    private InterfaceCarDrive4W interfaceCarDrive4W;
    
    public override void SetupAbility()
    {
        base.SetupAbility();
        interfaceCarDrive4W = GetComponentInParent<InterfaceCarDrive4W>();
        

    }

    public override void ResetAbility()
    {
        base.ResetAbility();
        DeactivateAbility();

    }


    public override void ActivateAbility()
    {
        if (!isSetup)
        {
            SetupAbility();
        }

        currentCharge = maxCharge;
        abilityActivated = true;


        
        
        abilityPhotonView.RPC(nameof(ActivationEffects_RPC), RpcTarget.All);
    }
    public override void DeactivateAbility()
    {
        abilityActivated = false;

       
    }
    // sustained ability set in manager - called every while active
    public override void Fire()
    {
        if (CanUseAbility())
        {
            currentCooldown = cooldown;
            timeSinceLastFire = 0;
            UseCharge(chargeUsedPerFire);
            Transform me = interfaceCarDrive4W.transform;
            me.GetComponent<Rigidbody>().AddForce(me.forward * activationVelocityChange, ForceMode.VelocityChange);
           // abilityPhotonView.RPC(nameof(ActivationEffects_RPC), RpcTarget.All);
        }
    }

    
}
