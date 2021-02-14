using UnityEngine;
using System.Collections;


public class ProjectileScript : MonoBehaviour
{
    public GameObject impactParticle;
    public GameObject projectileParticle;
    public GameObject[] trailParticles;
    [HideInInspector]
    public Vector3 impactNormal; //Used to rotate impactparticle.

    private bool trueProjectile = false;
    private bool hasCollided = false;

    public float impactParticleVolume = 1f;
    public AudioClip hitSound;
    
    private Weapon.WeaponDamageDetails weaponDamageDetails = new Weapon.WeaponDamageDetails();

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
            if(trueProjectile)DamageCollisionHandler();
            VisualCollisionHandler();
        }
    }

    // applies damage to the enemy (if we hit an enemy)
    
    // TODO - get health and apply damage
    void DamageCollisionHandler()
    {
        // get health script and apply weaponDamageDetails in the applydamage function
        
    }
    
    // destroys gameobject and does impact effects and such
    void VisualCollisionHandler()
    {
        // calculate
        hasCollided = true;
        impactParticle = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
        if (impactParticle.GetComponent<AudioSource>() != null && hitSound!=null)
        {
            impactParticle.GetComponent<AudioSource>().PlayOneShot(hitSound, impactParticleVolume);
        }

        foreach (GameObject trail in trailParticles)
        {
            GameObject curTrail = transform.Find(projectileParticle.name + "/" + trail.name).gameObject;
            curTrail.transform.parent = null;
            Destroy(curTrail, 3f);
        }
        Destroy(projectileParticle, 3f);
        Destroy(impactParticle, 5f);
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