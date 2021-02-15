using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class ProjectileWeapon : Weapon
{

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    
    public float projectileSpeed = 100f;
    public bool inheritVelocityFromVehicle = false;
    private Rigidbody parentRigidbody;

    

    
    
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
    public override void Fire(Vector3 startPoint, Vector3 targetPoint)
    {
        if (CanFire() && gunnerPhotonView.IsMine)
        {
            currentCooldown = fireRate;
            UseAmmo(ammoPerShot);
            float distanceMultiplier = CalculateDamageMultiplierCurve(Vector3.Distance(startPoint, targetPoint));
            // define weapon damage details
            WeaponDamageDetails weaponDamageDetails = new WeaponDamageDetails(myNickName, myPlayerId ,damageType, baseDamage*distanceMultiplier);
            string weaponDamageDetailsJson = JsonUtility.ToJson(weaponDamageDetails);
            weaponPhotonView.RPC(nameof(FireRPC_ProjectileWeapon), RpcTarget.All, startPoint, targetPoint, weaponDamageDetailsJson);
            // do the rest in subclass
        }
    }

    // only called on success
    // deals with firing the actual projectiles, and lag compensated dummy ones
    // RENAME THIS METHOD AS PER THE NAMING CONVENTION TO AVOID RPC SHENANIGANS
    // Convention: FireRPC_ClassName
    [PunRPC]
    protected new void FireRPC_ProjectileWeapon(Vector3 barrelEnd, Vector3 targetPoint, string serializedDamageDetails)
    {
        WeaponDamageDetails weaponDamageDetails = JsonUtility.FromJson<WeaponDamageDetails>(serializedDamageDetails);
        parentRigidbody = transform.root.GetComponent<Rigidbody>();
        // debug function to fire weapon
        weaponAnimator.SetTrigger(primaryFireAnimatorTriggerName);
        Debug.Log("ProjectileWeapon class object has fired");

        // if we are the owner of the photonview, then fire the real projectile
        GameObject projectile = Instantiate(projectilePrefab, barrelTransform.position, barrelTransform.rotation);
        ProjectileScript projScript = projectile.GetComponent<ProjectileScript>();
        projScript.impactParticle = imapactParticle;
        projScript.missImpactParticle = missImpactParticle;
        StopProjectileCollisionsWithSelf(projectile);
        DoMuzzleFlashEffect();
        projectile.transform.LookAt(targetPoint);
        projectile.transform.position -= projectile.transform.forward;
        // set projscript stuff
        projScript.SetWeaponDamageDetails(weaponDamageDetails);
        projScript.impactParticleVolume = imapactParticleVolume;
        projScript.missImpactParticleVolume = missImpactParticleVolume;
        projScript.hitSound = impactParticleSound;
        projScript.missSound = impactParticleSoundMiss;
        PlayAudioClipOneShot(weaponFireSound);
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
        
    }
}
