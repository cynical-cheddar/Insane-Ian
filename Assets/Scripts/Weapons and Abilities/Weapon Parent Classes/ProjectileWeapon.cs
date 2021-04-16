using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class ProjectileWeapon : Weapon
{

    [Header("Projectile Settings")]
  //  public PooledObject projectilePrefab;   ------------------ enable and implement if we need pooling
    public GameObject projectilePrefab;
    public GameObject projectileParticleEffectPrefab;
    public float projectileMass = 10f;
    public float projectileSpeed = 100f;
    public bool inheritVelocityFromVehicle = false;
    protected Rigidbody parentRigidbody;

    

    
    
    protected void DoMuzzleFlashEffect()
    {
        // instantiate muzzleflash
        GameObject mFlash = Instantiate(muzzleflash, barrelEndMuzzleTransform.position, barrelEndMuzzleTransform.rotation);
        if (mFlash.GetComponent<AudioSource>() != null) mFlash.GetComponent<AudioSource>().volume = muzzleflashVolume;
        mFlash.transform.parent = barrelEndMuzzleTransform;
        Destroy(mFlash, 1f);
    }

    protected void StopProjectileCollisionsWithSelf(GameObject projectile)
    {
        Collider[] parentColliders = transform.root.gameObject.GetComponentsInChildren<Collider>();
        Collider projCollider = projectile.GetComponent<Collider>();
        foreach (Collider col in parentColliders)
        {
            Physics.IgnoreCollision(projCollider, col);
        }
    }

    
    
    
    // Start is called before the first frame update
    // ---------------------- COPY THESE FUNCTIONS FOR EACH CHILD CLASS --------------------------------//
    // ---------------------- RPCs DO NOT INHERIT FROM PARENT ------------------------------------------//
    
    // method called by weaponController
    // we need a new version of this for every child class, otherwise the top level RPC will be called
    public override void Fire(Vector3 targetPoint)
    {
        Debug.LogWarning("Projectile Weapon has not been ported to the new PhysX system");
        return;

        if (CanFire() && gunnerPhotonView.IsMine)
        {
            targetPoint =
                CalculateFireDeviation(targetPoint, projectileDeviationDegrees);
            currentCooldown = fireRate;
            UseAmmo(ammoPerShot);
       //     float distanceMultiplier = CalculateDamageMultiplierCurve(Vector3.Distance(barrelTransform.position, targetPoint));
            // define weapon damage details
            WeaponDamageDetails weaponDamageDetails = new WeaponDamageDetails(myNickName, myPlayerId, myTeamId ,damageType, baseDamage, Vector3.zero);
            string weaponDamageDetailsJson = JsonUtility.ToJson(weaponDamageDetails);
            weaponPhotonView.RPC(nameof(FireRPC_ProjectileWeapon), RpcTarget.All, targetPoint, weaponDamageDetailsJson);
            // do the rest in subclass
        }
    }

    // only called on success
    // deals with firing the actual projectiles, and lag compensated dummy ones
    // RENAME THIS METHOD AS PER THE NAMING CONVENTION TO AVOID RPC SHENANIGANS
    // Convention: FireRPC_ClassName
    [PunRPC]
    protected virtual void FireRPC_ProjectileWeapon(Vector3 targetPoint, string serializedDamageDetails)
    {
        WeaponDamageDetails weaponDamageDetails = JsonUtility.FromJson<WeaponDamageDetails>(serializedDamageDetails);
        parentRigidbody = transform.root.GetComponent<Rigidbody>();
        // debug function to fire weapon
        weaponAnimator.SetTrigger(primaryFireAnimatorTriggerName);
      //  Debug.Log("ProjectileWeapon class object has fired");

        // if we are the owner of the photonview, then fire the real projectile

        
        /*
        PooledObject pooledProjectile =
            Pool.Instance.Spawn(projectilePrefab, barrelTransform.position, barrelTransform.rotation);


        GameObject projectile = pooledProjectile.gameObject;
        */

        GameObject obj = projectilePrefab;
        StopProjectileCollisionsWithSelf(obj);
        GameObject projectile = Instantiate(obj, barrelTransform.position, barrelTransform.rotation);
        StopProjectileCollisionsWithSelf(projectile);
        
        ProjectileScript projScript = projectile.GetComponent<ProjectileScript>();

        // set projscript stuff
        projScript.SetWeaponDamageDetails(weaponDamageDetails);
        projScript.ActivateProjectile( imapactParticle, missImpactParticle, projectileParticleEffectPrefab, impactParticleSound, impactParticleSoundMiss, imapactParticleVolume, missImpactParticleVolume);
        
        
        DoMuzzleFlashEffect();
        projectile.transform.LookAt(targetPoint);

        PlayAudioClipOneShot(weaponFireSound);
        projectile.GetComponent<Rigidbody>().mass = projectileMass;
        // FIRE REAL PROJECTILE
        if (gunnerPhotonView.IsMine)
        {
            projScript.SetTrueProjectile(true);
            projectile.GetComponent<Rigidbody>().AddForce(projectileSpeed *(projectile.transform.forward) , ForceMode.VelocityChange);
            if (inheritVelocityFromVehicle) projectile.GetComponent<Rigidbody>().AddForce(parentRigidbody.velocity, ForceMode.VelocityChange);
        }
        // add projectile settings 
        // otherwise fire a lag compensated dummy projectile with no damage scripts enabled
        else
        {
            projScript.SetTrueProjectile(false);
            float ping = (PhotonNetwork.GetPing() * 1.0f)/2;
            // update position by ping
            Vector3 newPos = projectile.transform.position + (projectile.transform.forward * (ping * 0.001f) * projectileSpeed);
            if (inheritVelocityFromVehicle)
            {
                newPos += parentRigidbody.velocity * ping * 0.001f;
            }

            projectile.transform.position = newPos;
            projectile.GetComponent<Rigidbody>().AddForce(projectileSpeed *(projectile.transform.forward) , ForceMode.VelocityChange);  
        }
        
        Destroy(projectile, 4f);
        //pooledProjectile.Finish(4f);
    }
}
