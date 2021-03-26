using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class NitroAbilityCharge : DriverAbility
{
    // Start is called before the first frame update
    public float activationVelocityChange = 5f;

    public GameObject loopingNitroPrefab;
    private GameObject loopingNitroInstance;

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

    [PunRPC]
    void nitro_RPC(bool set)
    {
        /*
        if (set)
        {
            if (loopingNitroPrefab != null)
            {
                loopingNitroInstance = Instantiate(loopingNitroPrefab, transform.position, transform.rotation);
                loopingNitroInstance.transform.parent = transform;
            }
        }
        else
        {
        */
          //  Destroy(loopingNitroInstance);

            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
            }

    }


    public override void ActivateAbility()
    {
        if (!isSetup)
        {
            SetupAbility();
        }

        currentCharge = maxCharge;
        abilityPhotonView.RPC(nameof(nitro_RPC), RpcTarget.All, true);

        abilityActivated = true;


        
        
        abilityPhotonView.RPC(nameof(ActivationEffects_RPC), RpcTarget.All);
    }
    public override void DeactivateAbility()
    {
        abilityPhotonView.RPC(nameof(nitro_RPC), RpcTarget.All, false);
        abilityActivated = false;
        Rigidbody rb = driverPhotonView.GetComponent<Rigidbody>();
        if (rb.transform.InverseTransformDirection(rb.velocity).z > interfaceCarDrive4W.maxSpeed)
        {
            Vector3 vel = rb.transform.InverseTransformDirection(rb.velocity);
            vel.z = interfaceCarDrive4W.maxSpeed;
            rb.velocity = rb.transform.TransformDirection(vel);
        }
        


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
            if(currentCharge == 0) DeactivateAbility();
           // abilityPhotonView.RPC(nameof(ActivationEffects_RPC), RpcTarget.All);
        }
    }

    
}
