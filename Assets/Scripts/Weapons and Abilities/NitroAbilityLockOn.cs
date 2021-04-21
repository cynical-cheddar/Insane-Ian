using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class NitroAbilityLockOn : DriverAbility
{
    // Start is called before the first frame update
    public float rammingSpeed = 60f;
    
    
    public GameObject loopingNitroPrefab;
    private GameObject loopingNitroInstance;

    private InterfaceCarDrive4W interfaceCarDrive4W;

    public float newMass = 4000f;
    public float newRammingDamageMultiplier = 8f;
    public float newRammingResistance = 20;

    private float oldMass = 4000;
    private float oldRammingResistance = 1;
    private float oldRammingDamageMultiplier = 1f;


    public override void SetupAbility()
    {
        base.SetupAbility();
        interfaceCarDrive4W = GetComponentInParent<InterfaceCarDrive4W>();
        // old vars
        GetComponent<PhotonView>().RPC(nameof(SaveVars), RpcTarget.AllBuffered);

    }

    [PunRPC]
    void SaveVars()
    {
        oldMass = driverPhotonView.GetComponent<PhysXRigidBody>().mass;
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
            driverPhotonView.GetComponent<PhysXRigidBody>().mass = newMass;
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
            driverPhotonView.GetComponent<PhysXRigidBody>().mass = oldMass;
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
        
        
        driverPhotonView.GetComponent<CollidableHealthManager>().rammingDamageResistance = newRammingResistance;
        
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
            PhysXRigidBody rb = driverPhotonView.GetComponent<PhysXRigidBody>();
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
          //  Debug.Log("fire ability");
            currentCooldown = cooldown;
            timeSinceLastFire = 0;
            UseCharge(chargeUsedPerFire);

            if (target!=transform.root) Debug.LogError("Lock on nitro needs porting");//StartCoroutine(CrashIntoTarget());
            else GetComponentInParent<PhysXRigidBody>().velocity = transform.forward * rammingSpeed;
            
            if (currentCharge == 0) Invoke(nameof(DeactivateAbility), cooldown);
            // abilityPhotonView.RPC(nameof(ActivationEffects_RPC), RpcTarget.All);
        }
    }

    private bool justCollided = false;
    public override void JustCollided()
    {
        if(abilityActivated) justCollided = true;
    }

    IEnumerator CrashIntoTarget()
    {
        //Debug.Log("CrashIntoTarget");
        Transform me = interfaceCarDrive4W.transform;
        PhysXRigidBody rb = me.GetComponentInParent<PhysXRigidBody>();
        PhysXRigidBody targetRb = target.GetComponent<PhysXRigidBody>();
        
        float elapsedTime = 0f;
       // rb.AddForce(transform.root.forward * rammingSpeed, ForceMode.VelocityChange); 
       float fractionMaxSpeed = transform.root.InverseTransformDirection(rb.velocity).z / interfaceCarDrive4W.maxSpeed;
        while (elapsedTime <= cooldown && !justCollided)
        {
            float estimatedTime = Vector3.Magnitude(target.position - transform.root.position) / rammingSpeed;
            Vector3 predictedPos = target.position + targetRb.velocity * (estimatedTime);
            
            

            Vector3 movePos = Vector3.Lerp(target.position, predictedPos, elapsedTime/cooldown);

            Vector3 moveDir = movePos - rb.position;
            
            // if the predicted pos is out of our sight, then break
            if (Mathf.Abs(Vector3.Angle(transform.root.forward,
                target.position - transform.root.position)) > 50)
            {
                break;
            }
           // direction.Normalize ();


        /*
            float angle = Vector3.SignedAngle(transform.root.forward, predictedPos - transform.root.forward,
                transform.root.up);
 
            Vector3 eulerAngleVelocity = new Vector3(0, angle, 0);
            Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity);
            deltaRotation *= torque;*/
        
            

            if (Vector3.Distance(transform.root.position, target.position) > 7)
            {
                Vector3 direction = movePos - transform.root.position;
                Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
                rb.rotation = Quaternion.RotateTowards(transform.root.rotation, toRotation, 10f * Time.deltaTime );
                
                
                Vector3 vel = moveDir.normalized *
                              (rammingSpeed * Mathf.Lerp(fractionMaxSpeed, 1, elapsedTime / cooldown));
                Vector3 relativeVel = transform.root.InverseTransformDirection(vel);
                relativeVel.y = -2;
                vel = transform.root.TransformDirection(relativeVel);
                rb.velocity = vel;
            }

            //  transform.root.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),  Time.deltaTime * 3);
           // transform.root.transform.LookAt(direction);
            
            



                
            
            yield return new WaitForFixedUpdate();
            elapsedTime += Time.deltaTime;
        }

        DeactivateAbility();
        justCollided = false;

    }

    
}
