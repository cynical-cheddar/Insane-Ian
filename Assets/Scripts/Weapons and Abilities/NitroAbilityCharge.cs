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

    public float newMass = 10000f;
    public float newRammingDamageMultiplier = 4f;
    public float newRammingResistance = 20;

    private float oldMass = 4000;
    private float oldRammingResistance = 1;
    private float oldRammingDamageMultiplier = 1f;
    public override void SetupAbility()
    {
        Debug.LogWarning("Nitro Ability Charge has not been ported to the new PhysX system");
        base.SetupAbility();
        interfaceCarDrive4W = GetComponentInParent<InterfaceCarDrive4W>();
        // old vars
        GetComponent<PhotonView>().RPC(nameof(SaveVars), RpcTarget.AllBuffered);

    }

    [PunRPC]
    void SaveVars()
    {
        oldMass = driverPhotonView.GetComponent<Rigidbody>().mass;
        oldRammingResistance = driverPhotonView.GetComponent<CollidableHealthManager>().rammingDamageResistance;
        oldRammingDamageMultiplier = driverPhotonView.GetComponent<CollidableHealthManager>().rammingDamageMultiplier;
    }

    public override void ResetAbility()
    {
        base.ResetAbility();
        DeactivateAbility();

    }

    [PunRPC]
    void nitro_RPC(bool set)
    {

        if (set)
        {

            // new vars
            driverPhotonView.GetComponent<Rigidbody>().mass = newMass;
            driverPhotonView.GetComponent<CollidableHealthManager>().rammingDamageResistance = newRammingResistance;
            driverPhotonView.GetComponent<CollidableHealthManager>().rammingDamageMultiplier = newRammingDamageMultiplier;
            if (loopingNitroPrefab != null)
            {
                loopingNitroInstance = Instantiate(loopingNitroPrefab, transform.position, transform.rotation);
                loopingNitroInstance.transform.parent = transform;
            }
        }
        else
        {

            //  Destroy(loopingNitroInstance);
            // old vars
            driverPhotonView.GetComponent<Rigidbody>().mass = oldMass;
            driverPhotonView.GetComponent<CollidableHealthManager>().rammingDamageResistance =oldRammingResistance;
            driverPhotonView.GetComponent<CollidableHealthManager>().rammingDamageMultiplier =oldRammingDamageMultiplier;

            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
            }

        }
    }
    


    public override void ActivateAbility()
    {
        
        
        
        
        if (driverPhotonView.IsMine)
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
    }
    public override void DeactivateAbility()
    {
        if (driverPhotonView.IsMine)
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
            if (currentCharge == 0) Invoke(nameof(DeactivateAbility), cooldown);
            // abilityPhotonView.RPC(nameof(ActivationEffects_RPC), RpcTarget.All);
        }
    }

    
}
