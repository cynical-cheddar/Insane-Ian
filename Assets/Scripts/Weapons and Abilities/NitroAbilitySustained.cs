using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class NitroAbilitySustained : DriverAbility
{
    // Start is called before the first frame update
    public float activationVelocityChange = 50f;
    [Header("Force Parameters")]
    [Range(12, 35)]
    public float maxSteerAngle = 20;
    [Range(1000, 80000)]
    public float motorTorque = 4500;
    [Range(1000, 80000)]
    public float brakeTorque = 8000;
    [Range(0, 30000)]
    public float brakeForce = 16000;
    [Range(0.001f, 0.5f)]
    public float steerRateLerp = 0.1f;
    [Range(0, 1)]
    public float baseExtremiumSlip = 0.3f;
    [Range(0,20000)]
    public float antiRollStiffness = 5000;

    private float oldMaxSteerAngle;
    private float oldMotorTorque;
    private float oldBrakeTorque;
    private float oldBrakeForce;
    private float oldSteerRateLerp;
    private float oldbaseExtremiumSlip;
    private float oldAntiRollStiffness;

    private InterfaceCarDrive4W interfaceCarDrive4W;
    
    public override void SetupAbility()
    {
        Debug.LogWarning("Nitro Ability Sustained has not been ported to the new PhysX system");
        base.SetupAbility();
        interfaceCarDrive4W = GetComponentInParent<InterfaceCarDrive4W>();
        
        oldMaxSteerAngle = interfaceCarDrive4W.maxSteerAngle;
        oldMotorTorque = interfaceCarDrive4W.motorTorque;
        oldBrakeTorque = interfaceCarDrive4W.brakeTorque;
        oldBrakeForce = interfaceCarDrive4W.brakeForce;
        oldbaseExtremiumSlip = interfaceCarDrive4W.baseExtremiumSlip;
        oldAntiRollStiffness = interfaceCarDrive4W.antiRollStiffness;
        
        interfaceCarDrive4W.maxSteerAngle = maxSteerAngle;
        interfaceCarDrive4W.motorTorque = motorTorque;
        interfaceCarDrive4W.brakeTorque = brakeTorque;
        interfaceCarDrive4W.brakeForce = brakeForce;
        interfaceCarDrive4W.baseExtremiumSlip = baseExtremiumSlip;
        interfaceCarDrive4W.antiRollStiffness = antiRollStiffness;
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
        oldMaxSteerAngle = interfaceCarDrive4W.maxSteerAngle;
        oldMotorTorque = interfaceCarDrive4W.motorTorque;
        oldBrakeTorque = interfaceCarDrive4W.brakeTorque;
        oldBrakeForce = interfaceCarDrive4W.brakeForce;
        oldbaseExtremiumSlip = interfaceCarDrive4W.baseExtremiumSlip;
        oldAntiRollStiffness = interfaceCarDrive4W.antiRollStiffness;
        
        interfaceCarDrive4W.maxSteerAngle = maxSteerAngle;
        interfaceCarDrive4W.motorTorque = motorTorque;
        interfaceCarDrive4W.brakeTorque = brakeTorque;
        interfaceCarDrive4W.brakeForce = brakeForce;
        interfaceCarDrive4W.baseExtremiumSlip = baseExtremiumSlip;
        interfaceCarDrive4W.antiRollStiffness = antiRollStiffness;

        Transform me = interfaceCarDrive4W.transform;
        me.GetComponent<Rigidbody>().AddForce(me.forward * activationVelocityChange, ForceMode.VelocityChange);
        
        abilityPhotonView.RPC(nameof(ActivationEffects_RPC), RpcTarget.All);
    }
    public override void DeactivateAbility()
    {
        abilityActivated = false;
        
        interfaceCarDrive4W.maxSteerAngle = oldMaxSteerAngle;
        interfaceCarDrive4W.motorTorque = oldMotorTorque;
        interfaceCarDrive4W.brakeTorque = oldBrakeTorque;
        interfaceCarDrive4W.brakeForce = oldBrakeForce;
        interfaceCarDrive4W.baseExtremiumSlip = oldbaseExtremiumSlip;
        interfaceCarDrive4W.antiRollStiffness = oldAntiRollStiffness;
       
    }
    // sustained ability set in manager - called every while active
    public override void Fire()
    {
        if (CanUseAbility())
        {
            currentCooldown = cooldown;
            timeSinceLastFire = 0;
            UseCharge(chargeUsedPerFire);
            
        }
    }

    
}
