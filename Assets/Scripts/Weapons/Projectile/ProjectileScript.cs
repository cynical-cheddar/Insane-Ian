using UnityEngine;
using System.Collections;


public class ProjectileScript : MonoBehaviour
{
    GameObject impactParticle;
    GameObject missImpactParticle;
    GameObject projectileParticle;
    GameObject projectileParticleInstance;

    [HideInInspector]
    public Vector3 impactNormal; //Used to rotate impactparticle.

    private bool trueProjectile = false;
    private bool hasCollided = false;

    float impactParticleVolume = 1f;
    float missImpactParticleVolume = 0.75f;
    AudioClip hitSound;
    AudioClip missSound;
    private Weapon.WeaponDamageDetails weaponDamageDetails = new Weapon.WeaponDamageDetails();

    VehicleManager hitVm;
    public void SetWeaponDamageDetails(Weapon.WeaponDamageDetails wdd)
    {
        weaponDamageDetails = wdd;
    }

    public void SetTrueProjectile(bool set)
    {
        trueProjectile = set;
    }
    
    public void ActivateProjectile(GameObject imp, GameObject misImp, GameObject projParticle, AudioClip hitS, AudioClip missS, float hitVol, float missVol)
    {
        impactParticle = imp;
        missImpactParticle = misImp;
        projectileParticle = projParticle;
        hitSound = hitS;
        missSound = missS;
        impactParticleVolume = hitVol;
        missImpactParticleVolume = missVol;
        
        projectileParticleInstance = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
        projectileParticleInstance.transform.parent = transform;
    }



    void OnCollisionEnter(Collision hit)
    {
        if (!hasCollided)
        {
            // if we are the true projectile, then deal with game altering stuff like damage n that
            impactNormal = hit.contacts[0].normal;
            hitVm = hit.gameObject.GetComponentInParent<VehicleManager>();
            if(trueProjectile)DamageCollisionHandler(hit);
            VisualCollisionHandler();
        }
    }
    // applies damage to the enemy (if we hit an enemy)
    

    void DamageCollisionHandler(Collision hit)
    {
        
        if (hitVm != null)
        {
            // call take damage rpc
            hitVm.TakeDamage(weaponDamageDetails);
        }

    }
    
    // destroys gameobject and does impact effects and such
    void VisualCollisionHandler()
    {
        // calculate
        hasCollided = true;
        
        
        // WE HAVE HIT A PLAYER, PLAY THE HIT PLAYER IMPACT STUFF
        if (hitVm != null)
        {
            GameObject impactParticleInstance = Instantiate(impactParticle, transform.position,
                Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
            if (impactParticleInstance.GetComponent<AudioSource>() != null && hitSound != null)
            {
                impactParticleInstance.GetComponent<AudioSource>().clip = hitSound;
                impactParticleInstance.GetComponent<AudioSource>().volume = impactParticleVolume;
                impactParticleInstance.GetComponent<AudioSource>().PlayOneShot(hitSound, impactParticleVolume);
            }
            Destroy(impactParticleInstance, 5f);
        }
        // WE HAVE NOT HIT A PLAYER
        else
        {
            GameObject missImpactParticleInstance = Instantiate(missImpactParticle, transform.position,
                Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
            if (missImpactParticleInstance.GetComponent<AudioSource>() != null && missSound != null)
            {
                missImpactParticleInstance.GetComponent<AudioSource>().clip = missSound;
                missImpactParticleInstance.GetComponent<AudioSource>().volume = missImpactParticleVolume;
                missImpactParticleInstance.GetComponent<AudioSource>().PlayOneShot(missSound, missImpactParticleVolume);
            }
            Destroy(missImpactParticleInstance, 5f);
        }

        Destroy(projectileParticleInstance, 3f);

        
        Destroy(gameObject);
			
        ParticleSystem[] trails = GetComponentsInChildren<ParticleSystem>();
        //Component at [0] is that of the parent i.e. this object (if there is any)
        for (int i = 1; i < trails.Length; i++)
        {
				
            ParticleSystem trail = trails[i];
				
            if (trail.gameObject.name.Contains("Trail"))
            {
                trail.transform.SetParent(null);
                Destroy(trail.gameObject, 2f);
            }
        }
    }
}