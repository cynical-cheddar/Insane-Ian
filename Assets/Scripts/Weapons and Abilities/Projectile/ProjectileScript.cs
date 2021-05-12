using UnityEngine;
using System.Collections;
using PhysX;


public class ProjectileScript : MonoBehaviour, ICollisionEnterEvent
{
    GameObject impactParticle;
    GameObject missImpactParticle;
    GameObject projectileParticle;
    GameObject projectileParticleInstance;

    private bool isTrueProjectile = false;

    float impactParticleVolume = 1f;
    float missImpactParticleVolume = 0.75f;
    AudioClip hitSound;
    AudioClip missSound;
    private Weapon.WeaponDamageDetails weaponDamageDetails = new Weapon.WeaponDamageDetails();

    public float explosionForce = 0.3f;
    public float explosionOffset = 1f;

    private PooledObject pooledObject;

    private bool firstInstantiation = true;

    PhysXRigidBody myRb;

    
    public void SetWeaponDamageDetails(Weapon.WeaponDamageDetails wdd)
    {
        weaponDamageDetails = wdd;
    }

    public void CollisionEnter() {}
    public bool requiresData { get { return true; } }

    public void SetTrueProjectile(bool set)
    {
        isTrueProjectile = set;
    }
    
    public void ActivateProjectile(GameObject imp, GameObject misImp, GameObject projParticle, AudioClip hitS, AudioClip missS, float hitVol, float missVol)
    {
        lastVel = GetComponent<PhysXRigidBody>().velocity;
        impactParticle = imp;
        missImpactParticle = misImp;
        projectileParticle = projParticle;
        hitSound = hitS;
        missSound = missS;
        impactParticleVolume = hitVol;
        missImpactParticleVolume = missVol;

        if (firstInstantiation)
            projectileParticleInstance =
                Instantiate(projectileParticle, transform.position, transform.rotation, transform) as GameObject;
        else projectileParticleInstance.transform.rotation = transform.rotation;
        firstInstantiation = false;
    }

    void Awake()
    {
        myRb = GetComponent<PhysXRigidBody>();
        pooledObject = GetComponent<PooledObject>();
    }
    
    Vector3 lastVel = Vector3.zero;

    protected void Update(){
        lastVel = myRb.velocity;
    }

    public void  CollisionEnter(PhysXCollision collision) {
    
       // Debug.LogError(collision.gameObject);
        
        Vector3 impactNormal = collision.GetContact(0).normal;

        // if we are the true projectile, then deal with game altering stuff like damage n that
        if (explosionForce > 0) {
            Squishing hitMeshSquisher = collision.gameObject.GetComponentInParent<Squishing>();
            if (hitMeshSquisher != null) {
                Vector3 explosionPoint = collision.GetContact(0).point + impactNormal * explosionOffset;
                hitMeshSquisher.ExplodeMeshAt(explosionPoint, explosionForce);
            }
        }

        Vector3 hitPoint = collision.GetContact(0).point;
        VehicleHealthManager hitVm = collision.gameObject.GetComponentInParent<VehicleHealthManager>();
        if(hitVm !=null){
            if(hitVm.teamId == weaponDamageDetails.sourceTeamId){
                Destroy(gameObject);
            }
        }
        if (isTrueProjectile) DamageCollisionHandler(hitVm, hitPoint);
        VisualCollisionHandler(impactNormal, hitVm != null);

     //   pooledObject.Finish();
        Destroy(gameObject);
    }
    
    // applies damage to the enemy (if we hit an enemy)
    private void DamageCollisionHandler(VehicleHealthManager hitVm, Vector3 hitpoint)
    {
        
        if (hitVm != null)
        {
            // call take damage rpc
            weaponDamageDetails.localHitPoint = hitVm.transform.InverseTransformPoint(hitpoint);
            hitVm.TakeDamage(weaponDamageDetails);
        }

    }
    
    // destroys gameobject and does impact effects and such
    private void VisualCollisionHandler(Vector3 impactNormal, bool hitPlayer)
    {
        
        if (hitPlayer)
        {
            PlayParticleEffect(impactParticle, impactNormal);
        }
        else
        {
            PlayParticleEffect(missImpactParticle, impactNormal);
        }
    }

    private void PlayParticleEffect(GameObject particle, Vector3 impactNormal) {
        
        // TODO - should load from pool
        
        GameObject particleInstance = Instantiate(particle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
        AudioSource particleAudio = particleInstance.GetComponent<AudioSource>();
        if (particleAudio != null && hitSound != null)
        {
            particleAudio.clip = hitSound;
            particleAudio.volume = impactParticleVolume;
            particleAudio.PlayOneShot(hitSound, impactParticleVolume);
        }
        Destroy(particleInstance, 5f);
    }
}