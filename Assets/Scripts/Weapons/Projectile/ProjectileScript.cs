using UnityEngine;
using System.Collections;


public class ProjectileScript : MonoBehaviour
{
    public GameObject impactParticle;
    public GameObject missImpactParticle;
    public GameObject projectileParticle;
    public GameObject[] trailParticles;
    [HideInInspector]
    public Vector3 impactNormal; //Used to rotate impactparticle.

    private bool trueProjectile = false;
    private bool hasCollided = false;

    public float impactParticleVolume = 1f;
    public float missImpactParticleVolume = 0.75f;
    public AudioClip hitSound;
    public AudioClip missSound;
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
    
    void Start()
    {
        projectileParticle = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
        projectileParticle.transform.parent = transform;
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
    
    // TODO - get health and apply damage
    void DamageCollisionHandler(Collision hit)
    {
        
        if (hitVm != null)
        {
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

        foreach (GameObject trail in trailParticles)
        {
            GameObject curTrail = transform.Find(projectileParticle.name + "/" + trail.name).gameObject;
            curTrail.transform.parent = null;
            Destroy(curTrail, 3f);
        }
        Destroy(projectileParticle, 3f);

        
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